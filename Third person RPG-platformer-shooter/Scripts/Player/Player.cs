using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingEntity
{

    //Singleton
    public static Player Instance;


    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Instance = this;

    }

}
