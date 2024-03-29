﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class ResourceSoundSampleProvider : ISampleProvider
    {
        public float Gain { get; set; }
        public bool Looping { get; set; }

        private readonly ResourceSound resourceSound;
        private long position;
        private const int tempBufferSize = 9600; //9600 = 200ms

        public ResourceSoundSampleProvider(ResourceSound resourceSound)
        {
            Gain = 1f;
            this.resourceSound = resourceSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (count > tempBufferSize)
                throw new Exception("Count too large for temp buffer");

            var availableSamples = resourceSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);

            if (Gain == 0.0f)
            {
                for (int i = 0; i < samplesToCopy; i++)
                {
                    buffer[i] = 0;
                }
            }
            else
            {
                Array.Copy(resourceSound.AudioData, position, buffer, 0, samplesToCopy);
                if (Gain != 1f)
                {
                    for (int i = 0; i < samplesToCopy; i++)
                    {
                        buffer[i] *= Gain;
                    }
                }
            }
            position += samplesToCopy;
            if (Looping)
            {
                if(samplesToCopy < count)
                {
                    //Inject a small amount of silence at the end before looping
                    Array.Clear(buffer, (int)samplesToCopy, (int)(count - samplesToCopy));//Ensure buffer is zeroed for silence so we don't get weird artifacts - sawbe
                    samplesToCopy = count;
                }
                if (position > resourceSound.AudioData.Length - 1)
                {
                    position = 0;
                    if (Gain != 0)
                        position = 0;
                }
            }
            return (int)samplesToCopy;
        }

        public void ResetToStart(float gain)
        {
            position = 0;
            Gain = gain;
        }

        public WaveFormat WaveFormat { get { return resourceSound.WaveFormat; } }
    }
}
