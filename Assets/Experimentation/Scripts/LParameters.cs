/////////////////////////////////////////////////////////////////////////////////////////
// 
//                 Université d'Aix Marseille (AMU) - 
//                 Centre National de la Recherche Scientifique (CNRS)
//                 Copyright © 2023 AMU, CNRS
//                 MIT Licence.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the “Software”), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions.
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
// The Software is provided “as is”, without warranty of any kind, express or implied, including but not limited
// to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall
// the authors or copyright holders AMU,CNRS be liable for any claim, damages or other liability, whether in an
// action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or
// other dealings in the Software.
//
// Except as contained in this notice, the name of the AMU, CNRS shall not be used in advertising or otherwise
// to promote the sale, use or other dealings in this Software without prior written authorization from the
// AMU, CNRS. 
//
// Author: Pergandi Jean-Marie - Institut des Sciences du  Mouvement - Centre de Réalité Virtuelle de la Méditerranée,
// jean-marie.pergandi@univ-amu.fr
//
//////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace Exp
{
    //Stock the parameters of the experimentation
    public class LParameters
    {
        //value of the parameters
        Dictionary<string, string> _values = new Dictionary<string, string>();
        public Dictionary<string, string> Values
        {
            get { return _values; }
        }

        //Constructor reads the Parameters.txt file. Don't use it, it's automatically called by the module.
        public LParameters()
        {
            if (!File.Exists(MExp.Inst.SubjectDirectory + "\\Parameters.txt"))
            {
                Debug.LogError(MExp.Inst.SubjectDirectory + "\\Parameters.txt not found");
                return;
            }
            StreamReader stream = new StreamReader(MExp.Inst.SubjectDirectory + "\\Parameters.txt");
            //read head
            while (!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                string[] values = line.Split('\t');
                _values[values[0]] = values[1];
            }

            if (MExp.Inst.SaveData)
            {
                //Test reading
                StreamWriter outStream = new StreamWriter(MExp.Inst.RecordingDirectory + "//outParameters.txt");
                for (int i = 0; i < _values.Count; i++)
                {
                    if (i < _values.Count - 1)
                        outStream.WriteLine(_values.ElementAt(i).Key + "\t" + _values.ElementAt(i).Value);//end of line
                    else
                        outStream.Write(_values.ElementAt(i).Key + "\t" + _values.ElementAt(i).Value);//end of file
                }
                outStream.Close();

                //Compare
                {
                    byte[] inContents = File.ReadAllBytes(MExp.Inst.SubjectDirectory + "\\Parameters.txt");
                    byte[] outContents = File.ReadAllBytes(MExp.Inst.RecordingDirectory + "\\outParameters.txt");
                    if (!inContents.SequenceEqual(outContents))
                        throw new Exception("The two files Parameters.txt and outParameters.txt are differents!");
                }
            }
        }
    }
}