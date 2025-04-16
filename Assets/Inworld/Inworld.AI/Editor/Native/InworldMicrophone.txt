class InworldMicrophone {
    constructor() {
        this.record = false;
        this.position = 0;
        this.sampleRate = this.getDeviceCaps()[0];
        this.duration = 0;
        this.initialized = false;
        this.permissionGranted = false;
        this.mediaAvailable = false;
        this.audioContext = null;
        this.requestingMedia = false;

        this.mediaStreamSource = null;
        this.audioWorkletNode = null;
        this.devicesList = [];
        this.deviceKinds = {
            VideoInput: "videoinput",
            AudioInput: "audioinput",
            AudioOutput: "audioinput"
        };
        this.delayedInitialize(this);
    }

    delayedInitialize(instance) {
        var canvas = document.querySelector("#unity-canvas");
        var handler = null;
        handler = () => {
            canvas.removeEventListener("touchstart", handler);
            canvas.removeEventListener("mousedown", handler);
            setTimeout(() => {
                instance.initialize(instance);
                instance.check();
            }, 100);
        };
        canvas.addEventListener("touchstart", handler);
        canvas.addEventListener("mousedown", handler);
    }

    async initialize(instance) {
        if (!instance.initialized) {
            instance.audioContext = new (window.AudioContext || window.webKitAudioContext)();
            await instance.audioContext.audioWorklet.addModule("./AudioResampler.js");
            instance.audioWorkletNode = new AudioWorkletNode(instance.audioContext, "inworld-audio-resampler");
            instance.audioWorkletNode.port.onmessage = event => {
                instance.nodeInputHandler(instance, event);
            };
            instance.initialized = true;
        }
    }

    async check() {
        await this.refreshDevices();
        await this.refreshDevices();
        setInterval(() => {
            this.permissionStatusHandler(this);
        }, 1000);
    }

    getDeviceCaps() {
        return [16000, 48000];
    }

    getPosition() {
        return this.position;
    }

    isRecording() {
        return this.record;
    }

    start(deviceId, sampleRate, loop, duration) {
        if (this.record === true || this.requestingMedia === true || this.initialized === false) {
            return;
        }
        this.sampleRate = sampleRate;
        this.position = 0;
        this.loop = loop;
        this.duration = duration;

        let frequencyParam = this.audioWorkletNode.parameters.get("frequency");
        frequencyParam.setValueAtTime(this.sampleRate, this.audioContext.currentTime);

        this.requestingMedia = true;

        if (navigator.mediaDevices.getUserMedia) {
            var constraints = null;
            constraints = (deviceId !== null && navigator.mediaDevices.getSupportedConstraints().deviceId) ? {
                audio: {
                    deviceId: {
                        exact: deviceId
                    }
                }
            } : {
                audio: true
            };
            navigator.mediaDevices.getUserMedia(constraints)
                .then(stream => {
                    this.mediaGranted(this, stream);
                })
                .catch(error => {
                    this.mediaFailed(this, error);
                });
        }
    }

    end() {
        if (this.record === false || this.requestingMedia === true || this.initialized === false || this.mediaStreamSource === null) {
            return;
        }
        let recordingParam = this.audioWorkletNode.parameters.get("recording");
        recordingParam.setValueAtTime(0, this.audioContext.currentTime);
        this.record = false;
        this.mediaAvailable = false;
        this.mediaStreamSource.mediaStream.getTracks().forEach(track => track.stop());

        this.mediaStreamSource.disconnect(this.audioWorkletNode);
        InworldMicrophone.log("end");
    }

    devices() {
        return this.devicesList;
    }

    devicePermitted(kind) {
        let devices = this.devices();
        let permitted = !!devices.find(device => device.kind === kind && !!device.label);
        return permitted;
    }

    mediaGranted(instance, stream) {
        let recordingParam = instance.audioWorkletNode.parameters.get("recording");
        recordingParam.setValueAtTime(1, instance.audioContext.currentTime);
        instance.mediaAvailable = true;
        instance.requestingMedia = false;
        instance.record = true;
        instance.mediaStreamSource = instance.audioContext.createMediaStreamSource(stream);
        instance.mediaStreamSource.connect(instance.audioWorkletNode);

        InworldMicrophone.log("start");
    }

    mediaFailed(instance, error) {
        instance.mediaAvailable = false;
        instance.requestingMedia = false;
        InworldMicrophone.log("media stream denied");
        InworldMicrophone.log(error);
    }

    async refreshDevices() {
        if (navigator.mediaDevices?.enumerateDevices) {
            if (!this.mediaAvailable) {
                try {
                    await navigator.mediaDevices.getUserMedia({ audio: true });
                } catch (error) {
                    this.devicesList = [];
                    return;
                }
            }
            var devices = await navigator.mediaDevices.enumerateDevices();
            this.devicesList = [];
            for (var i = 0; i < devices.length; i++) {
                if (devices[i].kind === this.deviceKinds.AudioInput) {
                    var deviceInfo = {
                        deviceId: devices[i].deviceId,
                        kind: devices[i].kind,
                        label: devices[i].label,
                        groupId: devices[i].groupId
                    };
                    this.devicesList.push(deviceInfo);
                }
            }
        }
    }

    nodeInputHandler(instance, event) {
        if (!instance.record || instance.position / instance.sampleRate >= instance.duration && !instance.loop) {
            return;
        }
        let data = event.data;
        if (data === undefined || data.data[0] === undefined) {
            return;
        }        
        let length = data.data[0].length;
        let audioData = data.data;
        let bufferLength = document.microphoneNative.samplesMemoryData.length;   
        let startPosition = instance.position;
        let count = 0;
        for (let i = 0; i < length; i++) 
        {           
            document.microphoneNative.samplesMemoryData[instance.position] = audioData[0][i];
            instance.position++;
            if (instance.position + 1 > bufferLength) {
                if (instance.loop) {
                    instance.position = 0;
                } else {
                    instance.position = Math.max(0, bufferLength - 1);
                    break;
                }
            }
            count++;
        }
        document.microphoneNative.unityCommand("StreamChunkReceived", startPosition + ":" + count + ":" + bufferLength);
    }

    async permissionStatusHandler(instance) {
        await instance.refreshDevices();
        let permitted = instance.devicePermitted(instance.deviceKinds.AudioInput);
        if (instance.permissionGranted !== permitted) {
            instance.setPermissionStatus(permitted);
        }
    }

    setPermissionStatus(granted) {
        this.permissionGranted = granted;
        document.microphoneNative.unityCommand("PermissionChanged", this.permissionGranted);
    }

    static log(message) {
        console.log("[Unity][WebGL][Microphone]: " + message);
    }
}
