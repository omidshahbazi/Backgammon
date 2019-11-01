using Assets.Scripts.GamePlayLogic.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ClientUtilities.Extensions;
using RTLTMPro;
using Assets.Scripts.GamePlayLogic;
using System;
using Assets.Scripts.GamePlayLogic.UserData;
using ClientUtilities.UI;
using ClientUtilities.ResourceManager;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public delegate void UISendChangeTurnEvent(bool IsRecivedFromNetwork);
    public delegate void UISendUndoActionEvent();
    public class InGameMenu : UIBase
    {

        public static event UISendChangeTurnEvent OnChangeTurnEventClick = null;
        public static event UISendUndoActionEvent OnUndoEventClick = null;

        private SimulationManager simInstance;
        private Image ofillBar;
        private Image ufillBar;
        private Image oAvatar;
        private Image uAvatar;
        private Image uPl;
        private Image oPl;


        private UIButton UndoButton;
        private UIButton changeTheTurn;
        private UIButton rolltheDice;

        private RTLTextMeshPro uName;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro oName;
        private RTLTextMeshPro oLevel;
        private RTLTextMeshPro turnText;

        private UITweenMover TurnPaneleffect;
        private float period;
        private float timeInterval;
        private bool isDiceRolled = false;

        protected override void Awake()
        {
            base.Awake();
        }


        protected override void SetUIRefrences()
        {
            base.SetUIRefrences();

            simInstance = SimulationManager.Instance;
            ofillBar = transform.FindDeep("OFillBar").GetComponent<Image>();
            ufillBar = transform.FindDeep("UFillBar").GetComponent<Image>();
            oAvatar = transform.FindDeep("OAvatar").GetComponent<Image>();
            uAvatar = transform.FindDeep("UAvatar").GetComponent<Image>();
            uPl = transform.FindDeep("UPlayerColor").GetComponent<Image>();
            oPl = transform.FindDeep("OPlayerColor").GetComponent<Image>();

            uName = transform.FindDeep("UName").GetComponent<RTLTextMeshPro>();
            uLevel = transform.FindDeep("ULevel").GetComponent<RTLTextMeshPro>();
            oName = transform.FindDeep("OName").GetComponent<RTLTextMeshPro>();
            oLevel = transform.FindDeep("OLevel").GetComponent<RTLTextMeshPro>();
            turnText = transform.FindDeep("TurnPanelText").GetComponent<RTLTextMeshPro>();

            UndoButton = transform.FindDeep("Undo").GetComponent<UIButton>();
            changeTheTurn = transform.FindDeep("ChangeTheTurn").GetComponent<UIButton>();
            rolltheDice = transform.FindDeep("RollTheDice").GetComponent<UIButton>();

            TurnPaneleffect = transform.FindDeep("TurnPanelTextPanel").GetComponent<UITweenMover>();

            UndoButton.onClick.AddListener(OnUndoActionClick);
            changeTheTurn.onClick.AddListener(OnChangeTurnClick);
            rolltheDice.onClick.AddListener(OnRollTheDiceClick);


        }


        protected override void OnEnable()
        {
            base.OnEnable();
            if (simInstance != null)
            {
                simInstance.OnDiceRolled += OnDiceChanged;
                simInstance.OnTableReady += Instance_OnTableReady;
            }

        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (simInstance != null)
            {
                simInstance.OnDiceRolled -= OnDiceChanged;
                simInstance.OnTableReady -= Instance_OnTableReady;
            }

        }



        protected override void Update()
        {

            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    MoveTurnFlag();
            //}
            UpdateFillBars();
            if (!TableManager.Instance.IsGameStarted || simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                UndoButton.gameObject.SetActive(false);
                changeTheTurn.gameObject.SetActive(false);
                rolltheDice.gameObject.SetActive(false);

                return;
            }

            base.Update();


            rolltheDice.gameObject.SetActive(!isDiceRolled);
            UndoButton.gameObject.SetActive(simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length != simInstance.Board.TurnDice.Moves.Length);
            changeTheTurn.gameObject.SetActive(simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length == 0);
        }

        private void Instance_OnTableReady()
        {
            MoveTurnFlag();
            turnText.text = simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor ? "نوبت شما " : "نوبت حريف";
            uName.text = UserInfoManager.Instance.User.UserName;
            uLevel.text = "سطح" + UserInfoManager.Instance.User.Level;
            uPl.sprite = simInstance.YourColor == Simulation.Data.Game.PlayerColors.Black ? GameResourceManager.Instance.LoadSprite("FirstBoard/BlackBeed") : GameResourceManager.Instance.LoadSprite("FirstBoard/WhiteBeed");
            oName.text = UserInfoManager.Instance.Opponnent.UserName;
            oLevel.text = "سطح" + UserInfoManager.Instance.Opponnent.Level;
            oPl.sprite = simInstance.YourColor == Simulation.Data.Game.PlayerColors.Black ? GameResourceManager.Instance.LoadSprite("FirstBoard/WhiteBeed") : GameResourceManager.Instance.LoadSprite("FirstBoard/BlackBeed");
            ResetFillBars();
            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
                OnRollTheDiceClick();
        }


        private void OnDiceChanged()
        {

            turnText.text = simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor ? "نوبت شما " : "نوبت حريف";
            MoveTurnFlag();
            ResetFillBars();

            isDiceRolled = false;
            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
                OnRollTheDiceClick();
        }

        private void OnRollTheDiceClick()
        {
            isDiceRolled = true;
            Dice.Instance.RollTheDice();
        }



        private void MoveTurnFlag()
        {

            if (simInstance.YourColor == Simulation.Data.Game.PlayerColors.White)
                TurnPaneleffect.OnAnimateInsideIn();
            else           
                TurnPaneleffect.OnAnimateInsideOut();


        }

        private void ResetFillBars()
        {
            ufillBar.fillAmount = ufillBar.fillAmount = 1;
            period = TableManager.Instance.SelectedTable.TurnTime;
            timeInterval = period - 1;
        }


        private void UpdateFillBars()
        {
            if (!TableManager.Instance.IsGameStarted)
                return;
            period -= Time.deltaTime;

            if (period > timeInterval)
                return;



            timeInterval = period - 1;
            if (simInstance.CurrentSimulator.Frame.Board.TurnColor == simInstance.YourColor)
            {
             LeanTween.value(ufillBar.fillAmount ,  period / TableManager.Instance.SelectedTable.TurnTime,0.5f).setOnUpdate(updateUFillBar);

            }
            else
            {
                LeanTween.value(ofillBar.fillAmount, period / TableManager.Instance.SelectedTable.TurnTime,0.5f).setOnUpdate(updateOFillBar);
            }
        }

        private void updateOFillBar(float obj)
        {
            ofillBar.fillAmount = period / TableManager.Instance.SelectedTable.TurnTime;
        }

        private void updateUFillBar(float obj)
        {
            ufillBar.fillAmount = obj;
        }

        private void OnUndoActionClick()
        {
            OnUndoEventClick?.Invoke();

        }

        private void OnChangeTurnClick()
        {
            OnChangeTurnEventClick?.Invoke(false);
        }
    }

}