using UnityEngine;

public class Ship : SelectableObj
{
    // Widget parameters
    [SerializeField] private int _HpMax;
    [SerializeField] private int _HpCur;
    [SerializeField] private int _meleeNum;
    [SerializeField] private int _meleeHpStart;
    [SerializeField] private int _meleeHpCur;
    [SerializeField] private int _rangeNum;
    [SerializeField] private int _rangeHpStart;
    [SerializeField] private int _rangeHpCur;
    [SerializeField] private int _artiNum;
    [SerializeField] private int _artiHpStart;
    [SerializeField] private int _artiHpCur;
    [SerializeField] private int _sailorNum;
    [SerializeField] private int _sailorHpStart;
    [SerializeField] private int _sailorHpCur;
    [SerializeField] private int _armorNum;
    [SerializeField] private int _armorHpStart;
    [SerializeField] private int _armorHpCur;


    protected override void Start()
    {
        base.Start();  // ֲûחמג באחמגמדמ Start()
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
