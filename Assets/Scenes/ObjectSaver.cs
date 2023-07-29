/****************************************************
    文件：ObjectSaver.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using SanAndreasUnity.Importing.Items.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectSaver
{
    public int DefinitionId=-1;
    public float LoacalPositionX = 0;
    public float LoacalPositionY = 0;
    public float LoacalPositionZ = 0;

    public float LoacalRotationX = 0;
    public float LoacalRotationY = 0;
    public float LoacalRotationZ = 0;
    public float LoacalRotationW = 1;

    public float LoacalScaleX = 0;
    public float LoacalScaleY = 0;
    public float LoacalScaleZ = 0;

    public int Id = 0;


    public string ModelName = "";
    public string TextureDictionaryName = "";
    public float DrawDist = 0;
    public ObjectFlag ObjectFlags=ObjectFlag.None;

    public int ObjectId = 0;
    public string LodGeometry = "";
    public int CellId = 0;
    public int LodIndex=0;
    public bool  IsLod;
    public ObjectSaver() { }

}
