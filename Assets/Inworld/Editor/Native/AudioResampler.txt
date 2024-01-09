class InworldAudioResampler extends AudioWorkletProcessor {
    static get parameterDescriptors() {
        return [
            { name: "frequency", defaultValue: 16000, minValue: 16000, maxValue: 48000 },
            { name: "channels", defaultValue: 1, minValue: 1, maxValue: 1 },
            { name: "recording", defaultValue: 0, minValue: 0, maxValue: 1 },
        ];
    }

    constructor() {
        super();
    }

    process(inputs, outputs, parameters) {
        if (parameters.recording[0] === 0) {
            return true;
        }
        var outputData = null;

        for (let inputIndex = 0; inputIndex < inputs.length; inputIndex++) {
            var input = [...inputs[inputIndex]]; 
            outputData = { channels: input.length, data: [] };

            input = this.changeBitrate(input, sampleRate, parameters.frequency[0], parameters.channels[0]);

            for (let channel = 0; channel < input.length; channel++) {
                outputData.data[channel] = input[channel];
            }

            this.port.postMessage(outputData);
        }

        return true;
    }

    changeBitrate(inputChannels, currentSampleRate, targetFrequency, targetChannels) {
        if (inputChannels === null || currentSampleRate === targetFrequency || inputChannels.length === 0) {
            return inputChannels;
        }

        for (var channel = 0; channel < targetChannels; channel++) {
            if (inputChannels[channel] !== undefined && inputChannels[channel].length >= 64 && inputChannels[channel].length < 4 * targetFrequency) {
                inputChannels[channel] = this.downsampleBitrate(inputChannels[channel], currentSampleRate, targetFrequency);
            }
        }

        return inputChannels;
    }

    downsampleBitrate(inputArray, currentSampleRate, targetFrequency) {
        if (inputArray === null || currentSampleRate === targetFrequency) {
            return inputArray;
        }

        var sampleRateRatio = currentSampleRate / targetFrequency;
        var newLength = Math.round(inputArray.length / sampleRateRatio);
        var downsampledArray = new Float32Array(newLength);

        for (var i = 0, j = 0; i < downsampledArray.length; ) {
            var nextIndex = Math.round((i + 1) * sampleRateRatio);
            var sum = 0, count = 0;

            for (var k = j; k < nextIndex && k < inputArray.length; k++) {
                sum += inputArray[k];
                count++;
            }

            downsampledArray[i] = sum / count;
            i++;
            j = nextIndex;
        }

        return downsampledArray;
    }
}

registerProcessor("inworld-audio-resampler", InworldAudioResampler);
