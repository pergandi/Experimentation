/////////////////////////////////////////////////////////////////////////////////////////
// 
//                 Université d'Aix Marseille (AMU) - 
//                 Centre National de la Recherche Scientifique (CNRS)
//                 Copyright © 2020 AMU, CNRS All Rights Reserved.
// 
//     These computer program listings and specifications, herein, are
//     the property of Université d'Aix Marseille and CNRS
//    shall not be reproduced or copied or used in whole or in part as
//     the basis for manufacture or sale of items without written permission.
//     For a license agreement, please contact:
//   <mailto: licensing@sattse.com> 
//
//
//
//    Author: Pergandi Jean-Marie - Laboratoire ISM - jean-marie.pergandi@univ-amu.fr
//
//////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.UI;

using Exp;//il faut déclarer le module Exp pour utiliser le module experimentation

//La tâche de cette expérimentation exemple est de déplacer le curseur (disque noir) via la souris le plus rapidement pour atteindre la cible uniquement 
//quand cette dernière devient verte après un signal(GO affiché à l'écran) . Si elle est rouge il ne faut pas bouger. 
//Il y a un chrono dans la scène pour donner le top départ (GO) pour atteindre la cible.
//Il y a donc deux phases dans un essai : la phase chrono et la phase atteinte de cible

//La class MExp permet d'accèder à toutes les fonctionnalités du module
//MExp.Inst.Subject permet d'accéder aux données du fichier Subject.txt du dossier courant défini dans le fichier ExperimentationExample.txt
//MExp.Inst.Parameters permet d'accéder aux données du fichier Parameter.txt du dossier courant défini dans le fichier ExperimentationExample.txt
//MExp.Inst.Protocol permet d'accéder aux données du fichier Protocol.txt du dossier courant défini dans le fichier ExperimentationExample.txt
//MExp.Inst.Protocol.CurrentTrial permet d'accèder aux données de l'essai courant et de le gérer.
//MExp.Inst.Data permet de créer un tableau de données et d'y insérer des données qui s'enregistrent automatiquement dans un dossier daté. 
//Il y a un fichier de données par essai.
//Un chrono "global" (le départ est le lancement de l'application) et un chrono "essai" (le départ est le debut de l'essai) sont 
//automatiquement ajoutés aux fichiers enregistrés (voir documentation)

public class MyExperimentation : MonoBehaviour
{
    public GameObject _cursor;//C'est le curseur manipulé par le sujet via la souris
    public GameObject _target;//C'est la cible dont on changera la position et la couleur selon l'essai courant défini dans le fichier Protocol.txt
    public Text _subjectText;//c'est le texte affiché dans la scène pour le sujet. On y affichera le chrono puis un GO pour donner le signal pour l'atteinte de cible
    public Text _protocolText;//c'est le texte affiché dans la scène pour voir l'avancement du protocole.

    //on initialise dans Start() notre manip
    void Start()
    {
        //On créé notre tableau de données qui va enregistrer plusieurs données dans l'ordre de construction des colonnes
        //Pour une position x, y, z on utilise le méthode AddColumnsForPosition qui va créer automatiquement 3 colonnes x, y et z étiquetté par "Cursor".
        //L'étiquette "Cursor" va être réutilisé pour y mettre plus tard la position courante du curseur
        MExp.Inst.Data.AddColumnsForPosition("Cursor");
        //Ici on va enregistrer les mouvements de la souris, on veut s'avoir si le sujet anticipe avant le GO. On créé deux colonnes
        MExp.Inst.Data.AddColumn("MouseX");
        MExp.Inst.Data.AddColumn("MouseY");
        //Ici on ajoute une colonne appelé "Phase" pour enregistrer dans quelle phase de l'essai on est (soit phase "chrono" soit phase "atteinte de cible")
        MExp.Inst.Data.AddColumn("Phase");
        //Enfin on ajoute une autre colonne qui stockera si la cible est atteinte ou non
        MExp.Inst.Data.AddColumn("TargetAchieved");

        //on démarre le protocole, le premier essai est prêt à être joué
        MExp.Inst.Protocol.StartExperimentation();//le premier essai est dans l'état TrialState.NotStarted
    }

    //Ici à chaque frame on fait tourner notre manip
    void Update()
    {
        //PREPARATION DU NOUVEL ESSAI : si l'essai n'a pas démarré, on initialise les paramètres de départs puis on démarre l'essai
        if (MExp.Inst.Protocol.CurrentTrial.State == TrialState.NotStarted)
        {
            //on récupère la position de la cible via MExp.Inst.Protocol.CurrentTrial.Variables avec le nom de la variable Target_pos_z
            Vector3 newPos = new Vector3(0, 0, float.Parse(MExp.Inst.Protocol.CurrentTrial.Variables["Target_pos_z"]));
            //on applique la nouvelle position de la cible
            _target.transform.position = newPos;
            //au début la cible est blanche, elle changera de couleur quand le GO s'affichera
            _target.GetComponent<Renderer>().materials[0].SetColor("_Color", Color.white);
            //on replace le cursor à 0, sa position de départ
            _cursor.transform.position = Vector3.zero;

            //les paramètres initaux ont été appliquées, on peut démarrer l'essai
            MExp.Inst.Protocol.CurrentTrial.Start();//l'état de l'essai passe à TrialState.Running
        }
        //Ici, l'essai tourne, il faut réaliser les interactions, l'enregistrement des données et définir les conditions d'un essai terminé
        else if (MExp.Inst.Protocol.CurrentTrial.State == TrialState.Running)
        {
            //ON FAIT LES INTERACTIONS
            //On récupère le paramètre EndChrono dans le fichier Parameters.txt converti en flottant
            float endChrono = float.Parse(MExp.Inst.Parameters.Values["EndChrono"]);
            //on détermine dans quelle phase on est (par défaut on est en phase "chrono"
            string phase = "chrono";
            //si le chrono est supérieur à EndChrono on passe en phase "Atteinte de Cible"
            if (MExp.Inst.Protocol.CurrentTrial.Chrono >= endChrono)
                phase = "PointingTarget";

            //si on est phase chrono on affiche le chrono
            if (phase == "chrono")
            {
                //le paramètre "0.0" c'est pour afficher la valeur avec un chiffre après la virgule
                _subjectText.text = MExp.Inst.Protocol.CurrentTrial.Chrono.ToString("0.0");
            }
            //sinon on est en phase "PointingTarget", il faut afficher GO, modifier le couleur de la cible, déplacer le curseur via la souris
            else
            {
                //si le chrono est supérieur à EndChrono on affiche GO pour que le sujet réalise son pointage si la cible est verte
                _subjectText.text = "GO!";

                //selon la variable Target_color on change la couleur de la cible
                if (MExp.Inst.Protocol.CurrentTrial.Variables["Target_color"] == "red")
                    _target.GetComponent<Renderer>().materials[0].SetColor("_Color", Color.red);
                else
                    _target.GetComponent<Renderer>().materials[0].SetColor("_Color", Color.green);

                //Ensuite on modifie la position du cursor via la souris
                //On récupère le paramètre MouseVelocity dans le fichier Parameters.txt converti en flottant
                float mouseVelocity = float.Parse(MExp.Inst.Parameters.Values["MouseVelocity"]);
                //on translate le cursor selon les mouvements de la souris et on multiplie par mouseVelocity
                _cursor.transform.Translate(new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y")) * mouseVelocity);
            }

            //on récupère l'information si il y a eu collision entre le curseur et la cible
            //le curseur a un script DetectCollision qui détecte si l'objet est entré en collision avec un autre objet
            bool targetAchieved = _cursor.GetComponent<DetectCollision>().IsCollided();

            //ON ENREGISTRE LES DONNEES (remarque : l'ordre d'écriture des données n'a pas d'importance et peut être différente de l'ordre de création des colonnes dans la méthode Start()
            //A chaque frame on créé une nouvelle ligne de données à enregistrer
            MExp.Inst.Protocol.CurrentTrial.NewLineRecording();
            //On écrit la nouvelle position du curseur dans le fichier de données en rappelant l'étiquette des colonnes déclarée dans la méthode Start(), ici "Cursor" 
            MExp.Inst.Data.WritePosition("Cursor", _cursor.transform.position);
            //On écrit si la cible a été atteinte
            MExp.Inst.Data.Write("TargetAchieved", targetAchieved.ToString());
            //On écrit la phase courante 
            MExp.Inst.Data.Write("Phase", phase);
            //On écrit les mouvements de souris 
            MExp.Inst.Data.Write("MouseX", Input.GetAxis("Mouse X").ToString());
            MExp.Inst.Data.Write("MouseY", Input.GetAxis("Mouse Y").ToString());
            //Fin d'enregistrement de la ligne (ou de la frame)
            MExp.Inst.Protocol.CurrentTrial.EndLineRecording();

            //ON REGARDE SI C'EST LA FIN D'UN ESSAI
            //si la cible est atteinte c'est la fin de l'essai
            if(targetAchieved)
            {
                //la cible est atteinte c'est la fin d'un essai
                MExp.Inst.Protocol.CurrentTrial.End(true);
            }
            //Sinon si la cible n'est pas atteinte et que 4 secondes se sont écoulés après le GO, c'est timeout on arrête l'essai
            else if (MExp.Inst.Protocol.CurrentTrial.Chrono >= endChrono + 4)
            {
                //si la cible est rouge l'essai est réussi.
                //si la cible est verte et qu'il y a timeout l'essai est raté mais on souhaite absolument que le sujet atteigne la cible quand elle est verte
                //on déclare alors l'essai échoué afin de le rejouer 
                //(le module gère l'ordre et donc à quel moment il faudra rejouer cet essai raté selon l'option choisi ReplayFailedTrialType dans le fichier Protocol.txt)
                bool success = true;
                if(MExp.Inst.Protocol.CurrentTrial.Variables["Target_color"] == "green")
                    success = false;
                //C'est la fin de l'essai, on passe en paramètre si l'essai est raté ou pas.
                MExp.Inst.Protocol.CurrentTrial.End(success);//l'état de l'essai passe à TrialState.Ended
            }
        }
        else if (MExp.Inst.Protocol.CurrentTrial.State == TrialState.Ended)
        {
            //C'est la fin d'un essai, on lance le suivant et la boucle est bouclée
            //Si NextTrial retourne "false" cela signifie que c'est la fin de l'experimentation
            if (!MExp.Inst.Protocol.NextTrial())
            {
                //si c'est la fin de la manip on l'affiche
                _subjectText.text = "Fin de la manip";
            }
        }

        //si il y a un essai courant on affiche quelques données
        if (MExp.Inst.Protocol.CurrentTrial!=null)
        {
            _protocolText.text = "Sujet N° : " + MExp.Inst.Subject.Attributes["Num"];
            _protocolText.text += "\nSujet ordre : " + MExp.Inst.Subject.Attributes["Order"];
            _protocolText.text += "\nNb d'essai : " + MExp.Inst.Protocol.TrialsCount;
            _protocolText.text += "\nNb block: " + MExp.Inst.Protocol.BlockCount;
            _protocolText.text += "\nTaille d'un block: " + MExp.Inst.Protocol.BlockSize;
            _protocolText.text += "\nEssai index : " + MExp.Inst.Protocol.CurrentTrial.Index;
            _protocolText.text += "\nEssai ordre : " + MExp.Inst.Protocol.CurrentTrial.Order;
            _protocolText.text += "\nBlock n°: " + MExp.Inst.Protocol.CurrentBlock;
            _protocolText.text += "\nEssai rejoué ? " + MExp.Inst.Protocol.ReplayFailedTrials;
            _protocolText.text += "\nNb essais à rejouer : " + MExp.Inst.Protocol.FailedTrialsCount;
            _protocolText.text += "\nQuand à rejouer ? " + MExp.Inst.Protocol.ReplayFailedTrialType;
            _protocolText.text += "\nPosition cible : " + MExp.Inst.Protocol.CurrentTrial.Variables["Target_pos_z"];
        }
    }
}
