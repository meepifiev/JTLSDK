var JTLSDKLibrary = {
    JTLSDK_IsAvailable: function () {
        return typeof Module.JTLSDK !== 'undefined' && Module.JTLSDK !== null ? 1 : 0;
    },

    JTLSDK_Register: function (callbackPointer) {
        if (typeof Module.JTLSDK === 'undefined' || Module.JTLSDK === null) {
            return;
        }

        Module.JTLSDK.register(callbackPointer);
    },

    JTLSDK_Call: function (modulePointer, actionPointer, payloadPointer, requestId) {
        if (typeof Module.JTLSDK === 'undefined' || Module.JTLSDK === null) {
            return;
        }

        Module.JTLSDK.call(UTF8ToString(modulePointer), UTF8ToString(actionPointer), UTF8ToString(payloadPointer), requestId);
    },

    JTLSDK_Query: function (modulePointer, actionPointer, payloadPointer) {
        var result = '{"code":1,"message":"bridge unavailable"}';

        if (typeof Module.JTLSDK !== 'undefined' && Module.JTLSDK !== null) {
            result = Module.JTLSDK.query(UTF8ToString(modulePointer), UTF8ToString(actionPointer), UTF8ToString(payloadPointer));
        }

        var size = lengthBytesUTF8(result) + 1;
        var buffer = _malloc(size);
        stringToUTF8(result, buffer, size);
        return buffer;
    }
};

mergeInto(LibraryManager.library, JTLSDKLibrary);
