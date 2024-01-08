var InworldMicrophoneInterop = 
{
    $CallbacksMap:{},

    WebGLInitSamplesMemoryData: function(byteOffset, length) 
    {
        document.microphoneNative.samplesMemoryData = new Float32Array(buffer, byteOffset, length);
    },

    WebGLGetDeviceData: function () 
    {
        if(document.microphoneNative === undefined)
            return document.microphoneNative.getPtrFromString("[]");
        var devices = document.microphoneNative.microphone.devices();
        return document.microphoneNative.getPtrFromString(JSON.stringify({ devices: devices }));
    },

    WebGLGetDeviceCaps: function() {
        if(document.microphoneNative === undefined)
            return document.microphoneNative.getPtrFromString("[]");
        var caps = document.microphoneNative.microphone.getDeviceCaps();
        return document.microphoneNative.getPtrFromString(JSON.stringify({ caps: caps }));
    },

    WebGLGetPosition: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.getPosition();
    },

    WebGLIsRecording: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.isRecording() ? 1 : 0;
    },

    WebGLMicEnd: function() {
        if(document.microphoneNative === undefined)
            return;
        document.microphoneNative.microphone.end();
    },

    WebGLMicStart: function(deviceId, frequency, lengthSec) {
        if(document.microphoneNative === undefined)
            return;
        document.microphoneNative.microphone.start(document.microphoneNative.getStringFromPtr(deviceId), frequency, 1, lengthSec);
    },

    WebGLIsPermitted: function() {
        if(document.microphoneNative === undefined)
            return 0;
        return document.microphoneNative.microphone.devicePermitted(document.microphoneNative.microphone.deviceKinds.AudioInput) ? 1 : 0;
    },

    WebGLDispose: function() {
        if(document.microphoneNative != undefined){
            document.microphoneNative.microphone = undefined;
            document.microphoneNative.getPtrFromString = undefined;
            document.microphoneNative.getStringFromPtr = undefined;
            document.microphoneNative.unityCommand = undefined;
            document.microphoneNative = undefined;

            CallbacksMap = {};
        }
    },
    
    WebGLInit: function(callbackJSON) {
        if (document.microphoneNative != undefined)
            return;
    
        document.microphoneNative = {};
    
        const JSONCallbackName = "callbackJSON";
    
        CallbacksMap[JSONCallbackName] = callbackJSON;
    
        function getStringFromPtr(ptr) {
            return UTF8ToString(ptr);
        }
    
        function getPtrFromString(str) {
            var bufferSize = lengthBytesUTF8(str) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(str, buffer, bufferSize);
            return buffer;
        }
    
        function unityCommand(command, object) {
            if (object == null || command == null)
                return;
    
            var ptrFunc = CallbacksMap[JSONCallbackName];
            var buffer = getPtrFromString(JSON.stringify({ command: { command: command, data: object } }));
    
            Module['dynCall_vi'](ptrFunc, buffer);
            _free(buffer);
        }
    
        document.microphoneNative.microphone = new InworldMicrophone();
        document.microphoneNative.getPtrFromString = getPtrFromString;
        document.microphoneNative.getStringFromPtr = getStringFromPtr;
        document.microphoneNative.unityCommand = unityCommand;
    }
};

autoAddDeps(InworldMicrophoneInterop, '$CallbacksMap');
mergeInto(LibraryManager.library, InworldMicrophoneInterop);