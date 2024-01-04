//    
//    MICROPHONE PRO
//    CURRENT VERSION 4.0.0
//    POWERED BY FROSTWEEP GAMES
//    PROGRAMMER ARTEM SHYRIAIEV
//    LAST UPDATE FEBRUARY 16 2023
//    

var MicrophoneNativeLibrary = {

    $CallbacksMap:{},

    initSamplesMemoryData: function(byteOffset, length, left) {
        if(left === 0)
            document.microphoneNative.samplesMemoryDataLeftChannel = new Float32Array(buffer, byteOffset, length);
        else
            document.microphoneNative.samplesMemoryDataRightChannel = new Float32Array(buffer, byteOffset, length);
    },

    devicesData: function () {
        if(document.microphoneNative === undefined)
            return document.microphoneNative.getPtrFromString("[]");
        var devices = document.microphoneNative.microphone.devices();
        return document.microphoneNative.getPtrFromString(JSON.stringify({ devices: devices }));
    },

    getDeviceCaps: function() {
        if(document.microphoneNative === undefined)
            return document.microphoneNative.getPtrFromString("[]");
        var caps = document.microphoneNative.microphone.getDeviceCaps();
        return document.microphoneNative.getPtrFromString(JSON.stringify({ caps: caps }));
    },

    getPosition: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.getPosition();
    },

    isRecording: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.isRecording() ? 1 : 0;
    },

    end: function() {
        if(document.microphoneNative === undefined)
            return;
        document.microphoneNative.microphone.end();
    },

    start: function(deviceId, frequency, loop, lengthSec) {
        if(document.microphoneNative === undefined)
            return;
        document.microphoneNative.microphone.start(document.microphoneNative.getStringFromPtr(deviceId), frequency, loop == 1, lengthSec);
    },

    isPermissionGranted: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.devicePermitted(document.microphoneNative.microphone.deviceKinds.AudioInput) ? 1 : 0;
    },

    setLeapSync: function(enabled) {
        if(document.microphoneNative === undefined)
            return;
        document.microphoneNative.microphone.setLeapSync(enabled === 1);
    },

    dispose: function() {
        if(document.microphoneNative != undefined){
            document.microphoneNative.microphone = undefined;
            document.microphoneNative.getPtrFromString = undefined;
            document.microphoneNative.getStringFromPtr = undefined;
            document.microphoneNative.unityCommand = undefined;
            document.microphoneNative = undefined;

            CallbacksMap = {};
        }
    },
    
    init: function(callbackJSON) {
        if(document.microphoneNative != undefined)
            return;

        document.microphoneNative = {};

        const JSONCallbackName = "callbackJSON";

        CallbacksMap[JSONCallbackName] = callbackJSON;

        function getStringFromPtr(ptr) {
            if (typeof UTF8ToString === "function")
                return UTF8ToString(ptr);
            else 
                return Pointer_stringify(ptr);
        }

        function getPtrFromString(str){
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(str, buffer, bufferSize);
            return buffer;
        }

        function unityCommand(command, object){
            if(object == null || command == null)
                return;

            var ptrFunc = CallbacksMap[JSONCallbackName];
            var buffer = getPtrFromString(JSON.stringify({ command: { command: command, data: object } }));

            if(typeof Runtime !== 'undefined' && typeof Runtime.dynCall === "function"){
                Runtime.dynCall('vi', ptrFunc, [buffer]);
            } else{
                Module['dynCall_vi'](ptrFunc, buffer);
            }
            
            _free(buffer);
        }

        document.microphoneNative.microphone = new InworldMicrophone();
        document.microphoneNative.getPtrFromString = getPtrFromString;
        document.microphoneNative.getStringFromPtr = getStringFromPtr;
        document.microphoneNative.unityCommand = unityCommand;
    }
};

autoAddDeps(MicrophoneNativeLibrary, '$CallbacksMap');
mergeInto(LibraryManager.library, MicrophoneNativeLibrary);