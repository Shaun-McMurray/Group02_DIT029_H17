﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTextMeshText : MonoBehaviour
{

    public string className;
    private TextMesh mesh;

    // Use this for initialization
    void Start()
    {
        className = "I AM CHANGABLE EVEN AFTER COMPILATION";
        display();

    }

    // Update is called once per frame
    void Update()
    {
        display();
    }

    void display()
    {
        mesh = GetComponent<TextMesh>();
        mesh.text = className;
    }
}