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
    //Manages the order of the trials.
    //The protocol manages the failed trials and can replay them according to many options:
    //- Do nothing
    //- Replay just after
    //- Replay at the end of the block
    //- Replay at the end of the experimentation
    //This option is choosen via the Protocol.txt file

    public class LProtocol
    {
        //head of the protocol array in Protocol.txt file
        List<String> _head = new List<string>();
        public List<string> Head { get { return _head; } }

        /************MANAGEMENT OF THE TRIALS*******************/
        //List of the trials of the protocol declared in the Protocol.txt file
        List<LTrial> _trials = new List<LTrial>();

        //Index of the current trial
        int _trialIndex = -1;
        public int TrialIndex
        {
            get { return _trialIndex; }
        }

        //return current trial
        public LTrial CurrentTrial
        {
            get
            {
                //if we are playing the failed trial, we return the first failed trial (the oldest) of the array _failedTrialIndexes
                if (_replayFailedTrials && _failedTrialIndexes.Count > 0)
                    return _trials[_failedTrialIndexes[0]];
                //if the experimentation is not started we return null
                if (_trialIndex < 0)
                    return null;
                //we return the current index
                return _trials[_trialIndex];
            }
        }

        //Nb of the trials
        public int TrialsCount
        {
            get{return _trials.Count;}
        }

        //Size of a block of trial
        int _blockSize;

        //Nb of block
        public int BlockCount
        {
            get { return _trials.Count / _blockSize; }
        }

        //Nb the trial by block. This option is choosen via the Protocol.txt file
        public int BlockSize
        {
            get { return _blockSize; }
        }

        //Index of the current block
        public int CurrentBlock
        {
            get { return Mathf.FloorToInt(_trialIndex / _blockSize); }
        }

        /************MANAGEMENT OF THE FAILED TRIALS*******************/
        //List of the indices of the failed trials
        List<int> _failedTrialIndexes = new List<int>();

        //the protocol is playing or not the failed trial
        bool _replayFailedTrials = false;
        public bool ReplayFailedTrials
        {
            get { return _replayFailedTrials; }
        }

        //Type of management of the failed trial
        ReplayFailedTrialType _replayFailedTrialType = ReplayFailedTrialType.DoNot;
        public ReplayFailedTrialType ReplayFailedTrialType
        {
            get { return _replayFailedTrialType; }
        }

        //Nb of the failed trials to be replayed
        public int FailedTrialsCount
        {
            get{return _failedTrialIndexes.Count;}
        }

        /************MANAGEMENT OF THE ORDER OF TRIALS*******************/
        //order of the trial played in the protocol
        int _order = -1;

        //start the protocol from this index. This option is choosen via the Protocol.txt file
        int _startIndex = 0;
        public int StartIndex
        {
            get { return _startIndex; }
        }

        //Start the experimentation
        public void StartExperimentation()
        {
            _trialIndex = _startIndex - 1;
            NextTrial();
        }

        //Go to the next trial
        //Return false if this is the end of the experimentation
        public bool NextTrial()
        {
            //if there's a current trial (may be we are at the beginning of the manip) and if the current trial has failed we have to do some stuff
            if (CurrentTrial!=null && !CurrentTrial.Success)
            {
                if (_replayFailedTrialType != ReplayFailedTrialType.DoNot)
                {
                    CurrentTrial.ResetState();
                    _failedTrialIndexes.Add(CurrentTrial.Index);
                }
            }

            //if we are playing the failed trials we check if there's again failed trial
            if(_replayFailedTrials)
            {
                _failedTrialIndexes.RemoveAt(0);//we remove the previous index
                if (_failedTrialIndexes.Count==0)//it was the last failed trial
                {
                    _replayFailedTrials = false;
                }
                else
                {
                    //we have to replay the next failed trial
                    _order++;
                    CurrentTrial.Order = _order;
                    return true;
                }
            }

            if (_replayFailedTrialType == ReplayFailedTrialType.AtTheEndOfBlock && (_trialIndex+1)%_blockSize == 0)//if we are at the end of blocks
            {
                if (_failedTrialIndexes.Count > 0)//if there's failed trials we have to play them
                {
                    _replayFailedTrials = true;
                }
                else
                {
                    if (_trialIndex == _trials.Count - 1)
                        return false;
                    _trialIndex++;//we continue to the next trial of the next block;
                }
            }
            else if(_replayFailedTrialType == ReplayFailedTrialType.AtTheEnd && _trialIndex == _trials.Count-1)//if we are at the end of manip
            {
                if (_failedTrialIndexes.Count > 0)//if there's failed trials we have to play them
                {
                    _replayFailedTrials = true;
                }
                else
                {
                    //it's finished! we have nothing to do
                    return false;//we don't have to affect the order at the end of this method
                }
            }
            else if (_replayFailedTrialType == ReplayFailedTrialType.JustAfter && CurrentTrial!=null && !CurrentTrial.Success)//if the current trial failed and we have to play it now
            {
                _replayFailedTrials = true;
            }
            else //else we continue as normal
            {
                if (_trialIndex == _trials.Count - 1)
                    return false;
                _trialIndex++;
            }

            //in every case, we increase the order
            _order++;
            CurrentTrial.Order = _order;
            return true;
        }

        //Constructor reads the Protocol.txt file and initialize the attributes of the class. Don't use it, it's automatically called by the module.
        public LProtocol()
        {
            StreamReader stream = new StreamReader(MExp.Inst.SubjectDirectory + "\\Protocol.txt");
            //read start index
            string line = stream.ReadLine();
            string[] values = line.Split('\t');
            _startIndex = int.Parse(values[1]);//read start index

            ///MAKE INDEX TRIAL AND INDEX BLOCKS FROM INDEX PROTOCOL

            line = stream.ReadLine();
            values = line.Split('\t');
            _blockSize = int.Parse(values[1]);

            line = stream.ReadLine();
            values = line.Split('\t');
            int repeatTypeInt = int.Parse(values[1]);
            switch(repeatTypeInt)
            {
                case 0:
                    _replayFailedTrialType = ReplayFailedTrialType.DoNot;
                    break;
                case 1:
                    _replayFailedTrialType = ReplayFailedTrialType.JustAfter;
                    break;
                case 2:
                    _replayFailedTrialType = ReplayFailedTrialType.AtTheEndOfBlock;
                    break;
                case 3:
                    _replayFailedTrialType = ReplayFailedTrialType.AtTheEnd;
                    break;
            }

            //read head
            line = stream.ReadLine();
            values = line.Split('\t');

            foreach (string str in values)
                _head.Add(str);

            //read tries
            int index = -1;
            while (!stream.EndOfStream)
            {
                line = stream.ReadLine();
                index++;
                values = line.Split('\t');
                _trials.Add(new LTrial(index, _head, values));
            }

            if (MExp.Inst.SaveData)
            {
                //Test reading
                StreamWriter outStream = new StreamWriter(MExp.Inst.RecordingDirectory + "\\outProtocol.txt");
                outStream.WriteLine("StartIndex\t"+_startIndex);
                outStream.WriteLine("BlockSize\t" + _blockSize);
                outStream.WriteLine("ReplayFailedTrialType\t" + (int)_replayFailedTrialType);
                for (int i = 0; i < _head.Count; i++)
                {
                    if (i < _head.Count - 1)
                        outStream.Write(_head[i] + "\t");
                    else outStream.WriteLine(_head[i]);
                }

                for (int i = 0; i < _trials.Count; i++)
                {
                    for (int j = 0; j < _trials[i].Variables.Count; j++)
                    {
                        if (j < _trials[i].Variables.Count - 1)
                            outStream.Write(_trials[i].Variables[_head[j]] + "\t");//in the line
                        else if (i < _trials.Count - 1)
                            outStream.WriteLine(_trials[i].Variables[_head[j]]);//end of line
                        else
                            outStream.Write(_trials[i].Variables[_head[j]]);//end of the file
                    }
                }
                outStream.Close();
                //Compare
                {
                    byte[] inContents = File.ReadAllBytes(MExp.Inst.SubjectDirectory + "\\Protocol.txt");
                    byte[] outContents = File.ReadAllBytes(MExp.Inst.RecordingDirectory + "\\outProtocol.txt");
                    if (!inContents.SequenceEqual(outContents))
                        throw new Exception("The two files Protocol.txt and outProtocol.txt are differents!");
                }
            }
            if (_blockSize < 0)
                _blockSize = _trials.Count;
        }
    }

    public enum ReplayFailedTrialType
    {
        DoNot,
        JustAfter,
        AtTheEndOfBlock,
        AtTheEnd
    }
}