using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Ship : SelectableObj
{
    // Widget params
    [SerializeField] private int _hpMax = 100;
    [SerializeField] private int _hpCur = 100;
    [SerializeField] private int _meleeNum = 0;
    [SerializeField] private int _meleeHpStart;
    [SerializeField] private int _meleeHpCur;
    [SerializeField] private int _rangeNum = 0;
    [SerializeField] private int _rangeHpStart;
    [SerializeField] private int _rangeHpCur;
    [SerializeField] private int _artiNum = 0;
    [SerializeField] private int _artiHpStart;
    [SerializeField] private int _artiHpCur;
    [SerializeField] private int _sailorNum = 0;
    [SerializeField] private int _sailorHpStart;
    [SerializeField] private int _sailorHpCur;
    [SerializeField] private int _armorNum = 0;
    [SerializeField] private int _armorHpStart;
    [SerializeField] private int _armorHpCur;

    // View params
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Canvas _widgetCanvas;
    [SerializeField] private UnityEngine.UI.Slider _hpBar; // Слайдер полоска здоровья
    [SerializeField] private Sprite _greenState;
    [SerializeField] private Sprite _yellowState;
    [SerializeField] private Sprite _redState;
    [SerializeField] private Sprite _blackState;
    private UnityEngine.UI.Button _meleeIndicator;
    private UnityEngine.UI.Button _rangeIndicator;
    private UnityEngine.UI.Button _artiIndicator;
    private UnityEngine.UI.Button _sailorIndicator;
    private UnityEngine.UI.Button _armorIndicator;
    private UnityEngine.UI.Text _meleeIndicatorText;
    private UnityEngine.UI.Text _rangeIndicatorText;
    private UnityEngine.UI.Text _artiIndicatorText;
    private UnityEngine.UI.Text _sailorIndicatorText;
    private UnityEngine.UI.Text _armorIndicatorText;

    protected override void Start()
    {
        base.Start();
        // Инициализация отображения виджета
        _hpMax = 400;
        _hpCur = 400;
        _meleeNum = 2;
        _rangeNum = 2;
        _artiNum = 2;
        _sailorNum = 2;
        _armorNum = 2;
        if ( _hpBar != null && _widgetCanvas != null ) {
            // Показатели здоровья
            _hpBar.interactable = false;
            _hpBar.minValue = 0;
            _hpBar.maxValue = _hpMax;
            _widgetCanvas.transform.rotation = _mainCamera.transform.rotation;
            _hpBar.onValueChanged.AddListener(OnSliderChanged);

            // Показатели юнитов
            _meleeIndicator = transform.Find("Canvas/Panel/MeleeIndicator").GetComponent<UnityEngine.UI.Button>();
            if (_meleeIndicator != null) _meleeIndicatorText = _meleeIndicator.GetComponentInChildren<Text>();
            if (_greenState != null && _meleeIndicator != null) _meleeIndicator.image.sprite = _greenState;
            if (_meleeIndicatorText != null) _meleeIndicatorText.text = _meleeNum.ToString();

            _rangeIndicator = transform.Find("Canvas/Panel/RangeIndicator").GetComponent<UnityEngine.UI.Button>();
            if (_rangeIndicator != null) _rangeIndicatorText = _rangeIndicator.GetComponentInChildren<Text>();
            if (_greenState != null && _rangeIndicator != null) _rangeIndicator.image.sprite = _greenState;
            if (_rangeIndicatorText != null) _rangeIndicatorText.text = _rangeNum.ToString();

            _artiIndicator = transform.Find("Canvas/Panel/ArtiIndicator").GetComponent<UnityEngine.UI.Button>();
            if (_artiIndicator != null) _artiIndicatorText = _artiIndicator.GetComponentInChildren<Text>();
            if (_greenState != null && _artiIndicator != null) _artiIndicator.image.sprite = _greenState;
            if (_artiIndicatorText != null) _artiIndicatorText.text = _artiNum.ToString();

            _sailorIndicator = transform.Find("Canvas/Panel/SailorIndicator").GetComponent<UnityEngine.UI.Button>();
            if (_sailorIndicator != null) _sailorIndicatorText = _sailorIndicator.GetComponentInChildren<Text>();
            if (_greenState != null && _sailorIndicator != null) _sailorIndicator.image.sprite = _greenState;
            if (_sailorIndicatorText != null) _sailorIndicatorText.text = _sailorNum.ToString();

            _armorIndicator = transform.Find("Canvas/Panel/ArmorIndicator").GetComponent<UnityEngine.UI.Button>();
            if (_armorIndicator != null) _armorIndicatorText = _armorIndicator.GetComponentInChildren<Text>();
            if (_greenState != null && _armorIndicator != null) _armorIndicator.image.sprite = _greenState;
            if (_armorIndicatorText != null) _armorIndicatorText.text = _armorNum.ToString();
        } else
        {
            Debug.LogError(transform.name + ": setup Canvas and Slider for the ship");
        }
        
    }

    void Update()
    {
        // Выравниваем виджет относительно камеры
        if (_hpBar != null && _widgetCanvas != null)
        {
            if (_widgetCanvas.transform.rotation != _mainCamera.transform.rotation)
            {
                _widgetCanvas.transform.rotation = _mainCamera.transform.rotation;
            }
        }   
    }

    // Уменьшение HP
    public void DescHp(int value)
    {
        _hpCur -= value;
        if (_hpCur < 0) _hpCur = 0;
        _hpBar.value = _hpCur;
    }

    void OnSliderChanged(float value)
    {
        Debug.Log("Новое значение здоровья: " + value);
    }
}
