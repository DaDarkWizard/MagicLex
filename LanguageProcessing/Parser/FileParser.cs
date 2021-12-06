using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LanguageProcessing.Parser
{
    public class FileParser
    {
        public int Position 
        { 
            get { return position; }
            set 
            {
                Seek(value, SeekOrigin.Begin);
            } 
        }
        private string text;
        private int position;
        public int Length { get; private set; }

        public FileParser(FileStream file)
        {
            var reader = new StreamReader(file);
            text = reader.ReadToEnd();
            Length = text.Length;
        }

        public int Peek()
        {
            return position < text.Length ? text[position] : -1;
        }

        public int ReadBlock(char[] buffer, int index, int count)
        {
            int i;
            for(i = 0; i + position < text.Length && i < count; i++)
            {
                buffer[i + index] = text[i + position];
            }
            position += i;
            return i;
        }

        public int Read()
        {
            int result = position < text.Length ? text[position] : -1;
            if(position <= text.Length) position++;
            return result;
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            if(origin == SeekOrigin.Begin)
            {
                position = offset;
            }
            else if(origin == SeekOrigin.Current)
            {
                position += offset;
            }
            else if(origin == SeekOrigin.End)
            {
                position = text.Length - offset - 1;
            }
        }
    }
}
