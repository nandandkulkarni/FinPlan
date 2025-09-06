window.saveAsFile = function (fileName, byteBase64) {
    var link = document.createElement('a');
    link.download = fileName;
    link.href = "data:application/octet-stream;base64," + byteBase64;
    try {
        document.body.appendChild(link);
        // Defensive click invocation
        if (typeof link.click === 'function') {
            try { link.click(); } catch (e) { console.error('saveAsFile click failed', e); }
        } else {
            // fallback: dispatch a MouseEvent
            try {
                var evt = new MouseEvent('click', { bubbles: true, cancelable: true, view: window });
                link.dispatchEvent(evt);
            } catch (e) { console.error('saveAsFile dispatchEvent failed', e); }
        }
    }
    catch (ex) { console.error('saveAsFile error', ex); }
    finally {
        try { if (link.parentNode) link.parentNode.removeChild(link); } catch (e) { /* ignore */ }
    }
}

window.downloadFileFromBase64 = function (base64String, mimeType, fileName) {
    // Create the blob with the appropriate MIME type
    try {
        const byteCharacters = atob(base64String);
        const byteArrays = [];

        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);

            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        const blob = new Blob(byteArrays, { type: mimeType });

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        try {
            document.body.appendChild(link);
            if (typeof link.click === 'function') {
                try { link.click(); } catch (e) { console.error('downloadFileFromBase64 click failed', e); }
            } else {
                try {
                    var evt = new MouseEvent('click', { bubbles: true, cancelable: true, view: window });
                    link.dispatchEvent(evt);
                } catch (e) { console.error('downloadFileFromBase64 dispatchEvent failed', e); }
            }
        } catch (ex) { console.error('downloadFileFromBase64 append/click failed', ex); }
        finally {
            try { window.URL.revokeObjectURL(url); } catch(e) { /* ignore */ }
            try { if (link.parentNode) link.parentNode.removeChild(link); } catch (e) { /* ignore */ }
        }
    } catch (e) {
        console.error('downloadFileFromBase64 error', e);
    }
}
