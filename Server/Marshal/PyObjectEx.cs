﻿using System.Collections.Generic;
using System.IO;

namespace Marshal
{
    public class PyObjectEx : PyObject
    {
        private const byte PackedTerminator = 0x2D;

        public bool IsType2 { get; set; }
        public PyObject Header { get; set; }
        public Dictionary<PyObject, PyObject> Dictionary { get; private set; }
        public List<PyObject> List { get; set; }

        public PyObjectEx(bool isType2, PyObject header)
            : base(PyObjectType.ObjectEx)
        {
            IsType2 = isType2;
            Header = header;
            Dictionary = new Dictionary<PyObject, PyObject>();
            List = new List<PyObject>();
        }

        public PyObjectEx()
            : base(PyObjectType.ObjectEx)
        {
            
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            if (op == MarshalOpcode.ObjectEx2)
                IsType2 = true;

            Dictionary = new Dictionary<PyObject, PyObject>();
            List = new List<PyObject>();
            Header = context.ReadObject(source);

            while (source.BaseStream.Position < source.BaseStream.Length)
            {
                if (source.ReadByte() == PackedTerminator)
                    break;
                source.BaseStream.Seek(-1, SeekOrigin.Current);
                List.Add(context.ReadObject(source));
            }

            while (source.BaseStream.Position < source.BaseStream.Length)
            {
                if (source.ReadByte() == PackedTerminator)
                    break;

                source.BaseStream.Seek(-1, SeekOrigin.Current);
                PyObject key = context.ReadObject(source);
                PyObject value = context.ReadObject(source);
                Dictionary.Add(key, value);
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(IsType2 ? MarshalOpcode.ObjectEx2 : MarshalOpcode.ObjectEx1);
            Header.Encode(output);

            // list
            if (List != null)
            {
                if (List.Count > 0)
                {
                    foreach (PyObject item in List)
                    {
                        item.Encode(output);
                    }
                }
            }

            output.Write(PackedTerminator);

            // dict
            if (Dictionary != null)
            {
                if (Dictionary.Count > 0)
                {
                    foreach (var pair in Dictionary)
                    {
                        pair.Key.Encode(output);
                        pair.Value.Encode(output);
                    }
                }
            }

            output.Write(PackedTerminator);
        }
    }
}