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
using System.IO;
using System;
using System.Linq;
using UnityEngine;

namespace Exp
{
    //Manage the data of the subject
    public class LSubject
    {
        //Stock the attributes of the subject declared in the Subject.txt file
        Dictionary<string, string> _attributes = new Dictionary<string, string>();
        public Dictionary<string, string> Attributes { get { return _attributes; } }

        //Constructor read the Subject.txt file and initialize the attributes of the class Subject. Don't use it, it's automatically called by the module.
        public LSubject()
        {
            if (!File.Exists(MExp.Inst.SubjectDirectory + "\\Subject.txt"))
            {
                Debug.LogError(MExp.Inst.SubjectDirectory + "\\Subject.txt not found");
                return;
            }
            StreamReader stream = new StreamReader(MExp.Inst.SubjectDirectory + "\\Subject.txt");

            while (!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                string[] values = line.Split('\t');
                _attributes[values[0]] = values[1];
            }
            stream.Close();

            if (MExp.Inst.SaveData)
            {

                //Test reading
                StreamWriter outStream = new StreamWriter(MExp.Inst.RecordingDirectory + "//outSubject.txt");
                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (i < _attributes.Count - 1)
                        outStream.WriteLine(_attributes.ElementAt(i).Key + "\t" + _attributes.ElementAt(i).Value);//end of line
                    else
                        outStream.Write(_attributes.ElementAt(i).Key + "\t" + _attributes.ElementAt(i).Value);//end of file
                }
                outStream.Close();
                //Compare
                {
                    byte[] inContents = File.ReadAllBytes(MExp.Inst.SubjectDirectory + "\\Subject.txt");
                    byte[] outContents = File.ReadAllBytes(MExp.Inst.RecordingDirectory + "\\outSubject.txt");
                    if (!inContents.SequenceEqual(outContents))
                        throw new Exception("The two files Subject.txt and outSubject.txt are differents!");
                }
            }
        }
    }
}