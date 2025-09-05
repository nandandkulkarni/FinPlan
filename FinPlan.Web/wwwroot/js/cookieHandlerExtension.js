window.cookieHandlerExtension = {
    GetCookie: function (name) {
        let match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? match[2] : null;
    },
    SetCookie: function (name, value, days) {
        let expires = "";
        if (days) {
            let date = new Date();
            date.setTime(date.getTime() + (days*24*60*60*1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    },

    // Store a JSON consent object as a cookie (and localStorage for convenience)
    SetConsentObject: function (key, obj, days) {
        try {
            const json = encodeURIComponent(JSON.stringify(obj));
            this.SetCookie(key, json, days);
            if (window.localStorage) window.localStorage.setItem(key, json);
        } catch (e) { console.error(e); }
    },

    GetConsentObject: function (key) {
        try {
            let raw = this.GetCookie(key);
            if (!raw && window.localStorage) raw = window.localStorage.getItem(key);
            if (!raw) return null;
            return JSON.parse(decodeURIComponent(raw));
        } catch (e) { return null; }
    },

    // Apply consent flags to the window for other scripts to check
    ApplyConsentToWindow: function (key) {
        try {
            var obj = this.GetConsentObject(key);
            if (obj) {
                window.fpConsent = obj;
                // convenience flags
                window.fpAnalyticsAllowed = !!obj.analytics;
                window.fpAdsPersonalization = !!obj.personalizedAds;
            }
        } catch (e) { }
    }
};
