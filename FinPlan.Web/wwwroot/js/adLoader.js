(function(){
    // Simple ad loader that lazy-loads placeholder 'ads' into reserved containers.
    // Respects consent flags set by cookieHandlerExtension.ApplyConsentToWindow

    function waitForCookieHandler(timeoutMs) {
        return new Promise((resolve) => {
            const start = Date.now();
            function check(){
                if (window.cookieHandlerExtension) return resolve(true);
                if (Date.now() - start > timeoutMs) return resolve(false);
                setTimeout(check, 100);
            }
            check();
        });
    }

    function createTestAdNode(container, personalized) {
        // create a simple iframe-like div so visual tests work without ad network
        const ad = document.createElement('div');
        ad.style.width = '100%';
        ad.style.height = '100%';
        ad.style.display = 'flex';
        ad.style.alignItems = 'center';
        ad.style.justifyContent = 'center';
        ad.style.background = personalized ? 'linear-gradient(90deg,#e6f2ff,#cfe9ff)' : 'linear-gradient(90deg,#f1f1f1,#e9e9e9)';
        ad.style.color = '#333';
        ad.style.border = '1px dashed rgba(0,0,0,0.06)';
        ad.style.boxSizing = 'border-box';
        ad.style.fontSize = '0.95rem';
        ad.innerText = personalized ? 'Ad (personalized)' : 'Ad (non-personalized)';
        // ensure focusable for accessibility
        ad.setAttribute('role','complementary');
        ad.setAttribute('aria-label','Advertisement');
        return ad;
    }

    function loadAdInto(container, consent) {
        try {
            if (!container) return;
            if (container.dataset.adLoaded) return;
            // Decide personalized vs non-personalized
            const personalized = !!(consent && consent.personalizedAds);
            // Create a test ad node sized to container
            const adNode = createTestAdNode(container, personalized);
            // Clear placeholder content and append
            container.innerHTML = '';
            container.appendChild(adNode);
            container.dataset.adLoaded = '1';
        } catch (e) { console.error('adLoader loadAdInto error', e); }
    }

    function init(options){
        options = options || {};
        const consentKey = options.consentKey || 'finplan_consent_v1';
        const selector = options.selector || '[data-ad-type]';
        const rootMargin = options.rootMargin || '200px 0px';

        waitForCookieHandler(3000).then((hasCookieHandler) => {
            // Apply stored consent to window flags if available
            try { if (hasCookieHandler) window.cookieHandlerExtension.ApplyConsentToWindow(consentKey); } catch(e){}

            const consent = (hasCookieHandler ? window.cookieHandlerExtension.GetConsentObject(consentKey) : null) || { analytics: false, personalizedAds: false };

            const els = Array.from(document.querySelectorAll(selector));
            if (!els.length) return;

            const observer = new IntersectionObserver((entries, obs) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const el = entry.target;
                        // Only load ad if user allowed analytics/ads or if we still want to show non-personalized
                        // We'll always show a non-personalized ad placeholder if personalized disallowed.
                        loadAdInto(el, consent);
                        obs.unobserve(el);
                    }
                });
            }, { root: null, rootMargin: rootMargin, threshold: 0.15 });

            els.forEach(el => {
                // if already loaded skip
                if (el.dataset.adLoaded) return;
                observer.observe(el);
            });
        });
    }

    // Expose simple API for ad push to avoid Blazor/AdSense DOM mutation conflicts.
    // Usage: call window.fpAds.pushAfterRender() from Blazor after the <ins> element is in the DOM.
    window.fpAds = window.fpAds || {};

    // New safer method that uses requestAnimationFrame to avoid DOM conflicts with Blazor
    window.fpAds.safeBlazorPush = function() {
        // Use requestAnimationFrame to wait for the browser to finish all rendering
        // This ensures Blazor's DOM updates are complete before AdSense tries to modify the DOM
        if (window.requestAnimationFrame) {
            window.requestAnimationFrame(function() {
                // Wait one more frame to be extra safe
                window.requestAnimationFrame(function() {
                    try {
                        if (window.adsbygoogle) {
                            window.adsbygoogle = window.adsbygoogle || [];
                            window.adsbygoogle.push({});
                            console.log('fpAds.safeBlazorPush: Successfully pushed ad');
                        } else {
                            console.warn('fpAds.safeBlazorPush: adsbygoogle not available yet');
                        }
                    } catch (e) {
                        console.error('fpAds.safeBlazorPush: Error pushing ad', e);
                    }
                });
            });
        } else {
            // Fallback for browsers without requestAnimationFrame
            setTimeout(function() {
                try {
                    if (window.adsbygoogle) {
                        window.adsbygoogle = window.adsbygoogle || [];
                        window.adsbygoogle.push({});
                        console.log('fpAds.safeBlazorPush: Successfully pushed ad (fallback)');
                    }
                } catch (e) {
                    console.error('fpAds.safeBlazorPush: Error in fallback', e);
                }
            }, 100);
        }
    };

    // Try to call (adsbygoogle=window.adsbygoogle||[]).push({}) when it's safe.
    window.fpAds.pushAfterRender = function(retries, delayMs) {
        retries = typeof retries === 'number' ? retries : 8;
        delayMs = typeof delayMs === 'number' ? delayMs : 120;

        function attempt(n) {
            try {
                // allow Blazor to finish DOM updates
                setTimeout(function() {
                    try {
                        if (window.adsbygoogle) {
                            try {
                                // Create the adsbygoogle array if it doesn't exist
                                window.adsbygoogle = window.adsbygoogle || [];
                                // Push the ad configuration
                                window.adsbygoogle.push({});
                                console.log('fpAds: Successfully pushed ad to adsbygoogle');
                                // success - no further retries needed
                                return;
                            } catch (e) {
                                // If adsbygoogle push throws, try again later
                                console.warn('fpAds.pushAfterRender: push error, retries left: ' + n, e);
                                if (n > 0) {
                                    attempt(n - 1);
                                }
                            }
                        } else {
                            console.warn('fpAds.pushAfterRender: adsbygoogle not ready, retries left: ' + n);
                            if (n > 0) {
                                attempt(n - 1);
                            } else {
                                console.error('fpAds.pushAfterRender: adsbygoogle never became available. Ad script may not have loaded.');
                            }
                        }
                    } catch(e) { 
                        console.warn('fpAds.pushAfterRender: error in attempt, retries left: ' + n, e);
                        if (n > 0) {
                            attempt(n - 1);
                        }
                    }
                }, delayMs);
            } catch (ex) { 
                console.error('fpAds.pushAfterRender: outer error', ex);
            }
        }

        attempt(retries);
    };

    // Helper function that can be safely called from Blazor via JSRuntime
    // This wraps pushAfterRender in a way that's compatible with Blazor's JSInterop
    window.fpAds.safePush = function() {
        try {
            if (window.fpAds && window.fpAds.pushAfterRender) {
                window.fpAds.pushAfterRender(8, 120);
            } else {
                console.error('fpAds.safePush: pushAfterRender not available');
            }
        } catch(e) {
            console.error('fpAds.safePush error:', e);
        }
    };

    window.adLoader = { init: init };

    console.log('adLoader.js: Initialized. fpAds methods available: safeBlazorPush, pushAfterRender, safePush');
})();
