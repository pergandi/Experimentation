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
using System.Collections;
using UnityEngine;

//Detect if there's a collision with another object
public class DetectCollision : MonoBehaviour
{
    bool _collided = false;

    private void OnTriggerEnter(Collider other)
    {
        _collided = true;
        StartCoroutine(SetFalse());
    }

    //put false at the next frame
    IEnumerator SetFalse()
    {
        yield return 0;
        _collided = false;
    }

    //Return if there's a collision
    public bool IsCollided()
    {
        return _collided;
    }
}
