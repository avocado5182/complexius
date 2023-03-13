using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [Flags]
public enum WeaponType {
    Single = 0b_0000_0001,
    Auto   = 0b_0000_0010,
    Burst  = 0b_0000_0100
}