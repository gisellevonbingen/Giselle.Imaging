using System;
using System.Collections.Generic;
using System.Text;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Riff
{
    public abstract class RiffChunkAbstractList : RiffChunk
    {
        public int FormType { get; set; }
        public string FormTypeToString => this.FormType.TypeKeyToString();
        public List<RiffChunk> Children { get; } = new List<RiffChunk>();

        public RiffChunkAbstractList()
        {

        }

        public override string ToString() => $"{this.FormTypeToString}, Children:{this.Children.Count}";

        protected override void ReadData(DataProcessor input)
        {
            this.FormType = input.ReadInt();
            this.Children.Clear();

            while (input.Position < input.Length)
            {
                this.Children.Add(ReadChunk(input.BaseStream));
            }

        }

        protected override void WriteData(DataProcessor output)
        {
            output.WriteInt(this.FormType);

            foreach (var element in this.Children)
            {
                WriteChunk(output.BaseStream, element);
            }

        }

    }

}
