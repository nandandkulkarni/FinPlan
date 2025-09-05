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

    window.adLoader = { init: init };
})();
