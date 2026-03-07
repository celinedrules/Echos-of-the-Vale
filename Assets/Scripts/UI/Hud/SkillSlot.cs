// Done
using System.Collections;
using Data.SkillData;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Enums;

namespace UI.Hud
{
    public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private SkillType skillType;
        [SerializeField] private Image cooldownImage;
        [SerializeField] private KeyCode inputKey = KeyCode.A;
        [SerializeField] private TextMeshProUGUI inputKeyText;
        [SerializeField] private GameObject conflictSlot;

        private UiManager _uiManager;
        private Image _icon;
        private RectTransform _rect;
        private Button _button;
        private SkillData _skillData;

        public SkillType SkillType => skillType;

        private void OnValidate()
        {
            gameObject.name = $"SkillSlot - {skillType.ToString()}";
        }

        private void Awake()
        {
            _uiManager = UiManager.Instance;
            _button = GetComponent<Button>();
            _icon = GetComponent<Image>();
            _rect = GetComponent<RectTransform>();
        }

        public void SetupSkillSlot(SkillData selectedSkill)
        {
            _skillData = selectedSkill;

            Color color = Color.black;
            color.a = 0.6f;
            cooldownImage.color = color;

            inputKeyText.text = GetInputKeyDisplayName();
            _icon.sprite = _skillData.Icon;

            if (conflictSlot)
                conflictSlot.SetActive(false);
        }

        public void StartCooldown(float cooldown)
        {
            cooldownImage.fillAmount = 1.0f;
            StartCoroutine(CooldownRoutine(cooldown));
        }

        public void ResetCooldown() => cooldownImage.fillAmount = 0.0f;

        private IEnumerator CooldownRoutine(float duration)
        {
            float timePassed = 0;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                cooldownImage.fillAmount = 1.0f - (timePassed / duration);
                yield return null;
            }

            cooldownImage.fillAmount = 0.0f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _uiManager.SkillTooltip.ShowTooltip(true, _rect, _skillData, null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _uiManager.SkillTooltip.ShowTooltip(false, null);
        }

        private string GetInputKeyDisplayName()
        {
            return inputKey switch
            {
                KeyCode.LeftShift => "L. SHIFT",

                KeyCode.Mouse0 => "LMB",
                KeyCode.Mouse1 => "RMB",
                KeyCode.Mouse2 => "MMB",
                _ => inputKey.ToString()
            };
        }
    }
}