// Safe browser information helper
(function () {
    window.fpBrowserInfo = window.fpBrowserInfo || {};

    // Safely get user agent without using eval
    window.fpBrowserInfo.getUserAgent = function () {
        try {
            return navigator.userAgent || '';
        } catch (e) {
            console.error('fpBrowserInfo.getUserAgent error:', e);
            return '';
        }
    };

    console.log('browserInfo.js: Initialized. fpBrowserInfo.getUserAgent is available.');
})();
