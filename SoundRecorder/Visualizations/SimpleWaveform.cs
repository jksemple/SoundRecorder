﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using SoundRecorder.Visualizations;


/// <summary>
/// A simple waveform visualization from the CSCore samples library.
/// </summary>
namespace SoundRecorder
{
    internal class SimpleWaveform : Visualization
    {
        private readonly List<float> _left = new List<float>();
        private readonly List<float> _right = new List<float>();

        private readonly object _lockObj = new object();

        override public void AddSamples(float left, float right)
        {
            lock (_lockObj)
            {
                _left.Add(left);
                _right.Add(right);
            }
        }

        override public Image Draw(int width, int height)
        {
            var image = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(image))
            {
                Draw(g, width, height);
            }
            return image;
        }

        override public void Draw(Graphics graphics, int width, int height)
        {
            const int pixelsPerSample = 2;
            var samplesLeft = GetSamplesToDraw(_left, width / pixelsPerSample).ToArray();
            var samplesRight = GetSamplesToDraw(_right, width / pixelsPerSample).ToArray();

            //left channel:
            graphics.DrawLines(new Pen(Color.DeepSkyBlue, 1), GetPoints(samplesLeft, pixelsPerSample, width, height).ToArray());
            //right channel:
            graphics.DrawLines(new Pen(Color.FromArgb(150, Color.Red), 0.5f), GetPoints(samplesRight, pixelsPerSample, width, height).ToArray());
        }

        private IEnumerable<Point> GetPoints(float[] samples, int pixelsPerSample, int width, int height)
        {
            int halfY = height / pixelsPerSample;
            if (samples.Length >= 2)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    Point point = new Point
                    {
                        X = i * pixelsPerSample,
                        Y = halfY + (int)(samples[i] * halfY)
                    };
                    yield return point;
                }
            }
            else
            {
                yield return new Point(0, halfY);
                yield return new Point(width, halfY);
            }
        }

        private IEnumerable<float> GetSamplesToDraw(List<float> inputSamples, int numberOfSamplesRequested)
        {
            float[] samples;
            lock (_lockObj)
            {
                samples = inputSamples.ToArray();
                inputSamples.Clear();
            }

            var validLength = samples.Length > 0;  // TODO: Necessary?
            var resolution = validLength ? (samples.Length / numberOfSamplesRequested) : 0;

            int index = 0;
            float currentMax = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                if (i > index * resolution)
                {
                    yield return currentMax;
                    currentMax = 0;
                    index++;
                }

                if (Math.Abs(currentMax) < Math.Abs(samples[i]))
                    currentMax = samples[i];
            }
        }
    }
}
