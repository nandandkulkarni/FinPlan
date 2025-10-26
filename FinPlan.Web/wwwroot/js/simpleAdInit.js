// Simple ad initialization for the SimpleAdTest page
window.simpleAdInit = {
    initialized: false,
    
    initialize: function() {
        if (this.initialized) {
            console.log('?? SimpleAdInit: Already initialized, skipping');
            return;
        }
        
        console.log('?? SimpleAdInit: Starting initialization...');
        
        // Check if adsbygoogle is available
        if (typeof window.adsbygoogle === 'undefined') {
            console.error('? SimpleAdInit: adsbygoogle not loaded! Check if ad blocker is enabled or script failed to load.');
            return false;
        }
        
        console.log('? SimpleAdInit: adsbygoogle is available');
        
        // Find all ad elements that haven't been initialized
        var ads = document.querySelectorAll('.adsbygoogle:not([data-adsbygoogle-status])');
        console.log('?? SimpleAdInit: Found ' + ads.length + ' uninitialized ad elements');
        
        if (ads.length === 0) {
            console.log('?? SimpleAdInit: No ads to initialize');
            return true;
        }
        
        // Initialize each ad with a delay between them to avoid conflicts
        var delay = 0;
        var successCount = 0;
        
        ads.forEach(function(ad, index) {
            setTimeout(function() {
                try {
                    console.log('?? SimpleAdInit: Initializing ad ' + (index + 1) + ' of ' + ads.length + '...');
                    
                    // Push to adsbygoogle array
                    (adsbygoogle = window.adsbygoogle || []).push({});
                    successCount++;
                    
                    console.log('? SimpleAdInit: Ad ' + (index + 1) + ' pushed to queue');
                } catch (e) {
                    console.error('? SimpleAdInit: Error initializing ad ' + (index + 1) + ':', e);
                }
            }, delay);
            delay += 250; // 250ms between each ad
        });
        
        // Log final status after all ads should be initialized
        setTimeout(function() {
            var queueLength = window.adsbygoogle ? window.adsbygoogle.length : 0;
            console.log('?? SimpleAdInit: Final status - Queue length: ' + queueLength + ', Expected: ' + ads.length);
            console.log('? SimpleAdInit: Initialization complete - ' + successCount + ' ads initialized');
        }, delay + 500);
        
        this.initialized = true;
        return true;
    },
    
    getStatus: function() {
        return {
            initialized: this.initialized,
            adsbyGoogleAvailable: typeof window.adsbygoogle !== 'undefined',
            queueLength: window.adsbygoogle ? window.adsbygoogle.length : 0,
            adElementCount: document.querySelectorAll('.adsbygoogle').length
        };
    }
};

console.log('? simpleAdInit.js loaded and ready');
