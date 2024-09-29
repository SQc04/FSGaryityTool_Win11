using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;
using System.IO;

namespace FSGaryityTool_Win11.Controls
{
    public class Player
    {
        private IWavePlayer _waveOutDevice;
        private WaveStream _mainOutputStream;

        public void PlayModFile(string modFilePath)
        {
            _waveOutDevice = new WaveOut();
            _mainOutputStream = CreateInputStream(modFilePath);
            _waveOutDevice.Init(_mainOutputStream);
            _waveOutDevice.Play();
        }

        private WaveStream CreateInputStream(string fileName)
        {
            WaveStream readerStream = new WaveFileReader(fileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }

            return readerStream;
        }
    }
}
