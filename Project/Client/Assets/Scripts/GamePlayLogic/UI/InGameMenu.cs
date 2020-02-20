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
using Assets.Scripts.ClientUtilities.ScheduleSystem;
using ClientUtilities.AudioMangaer;
using Networking.Common;

namespace Assets.Scripts.GamePlayLogic.UI
{
    public delegate void UISendChangeTurnEvent(bool IsRecivedFromNetwork);
    public delegate void UISendUndoActionEvent();
    public class InGameMenu : UIBase
    {

        public static event UISendChangeTurnEvent OnChangeTurnEventClick = null;
        public static event UISendUndoActionEvent OnUndoEventClick = null;

        private SimulationManager simInstance;

        private GameObject leavePanel;
        private GameObject autoRollDice;
        private GameObject connectionIsPoor;
        private Image ofillBar;
        private Image ufillBar;
        private Image oAvatar;
        private Image uAvatar;
        private Image uPl;
        private Image oPl;


        private UIButton UndoButton;
        private UIButton changeTheTurn;
        private UIButton rolltheDice;
        private UIButton OpenChatMenu;
        private UIButton diceOn;
        private UIButton diceOff;
        private UIButton resignButton;
        private UIButton okButton;
        private UIButton noButton;


        private RTLTextMeshPro uName;
        private RTLTextMeshPro uLevel;
        private RTLTextMeshPro oName;
        private RTLTextMeshPro oLevel;
        // private RTLTextMeshPro turnText;
        private RTLTextMeshPro chatText;

        private UITweenMover TurnPaneleffect;
        private UITweenMover ChatPanelEffect;

        private float period;
        private float timeInterval;
        private bool isDiceRolled = false;
        private bool IsAutoRoll = false;
        private int moveCount = 0;

        private Audio countDown;
        private Audio chatRecivedAudio;
        private bool isReplay;

        protected override void Awake()
        {
            base.Awake();
        }


        public override void SetUIRefrences()
        {
            if (IsRefrenceSet)
                return;


            simInstance = SimulationManager.Instance;

            leavePanel = transform.FindDeep("LeavePanel", true).gameObject;
            connectionIsPoor = transform.FindDeep("ConnectionIsPoor", true).gameObject;
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
            //  turnText = transform.FindDeep("TurnPanelText").GetComponent<RTLTextMeshPro>();
            chatText = transform.FindDeep("ChatText", true).GetComponent<RTLTextMeshPro>();


            UndoButton = transform.FindDeep("Undo").GetComponent<UIButton>();
            changeTheTurn = transform.FindDeep("ChangeTheTurn").GetComponent<UIButton>();
            rolltheDice = transform.FindDeep("RollTheDice").GetComponent<UIButton>();
            OpenChatMenu = transform.FindDeep("ChatButton").GetComponent<UIButton>();
            diceOn = transform.FindDeep("DiceOn", true).GetComponent<UIButton>();
            diceOff = transform.FindDeep("DiceOff", true).GetComponent<UIButton>();
            resignButton = transform.FindDeep("ResignButton").GetComponent<UIButton>();
            okButton = leavePanel.transform.FindDeep("OkButton", true).GetComponent<UIButton>();
            noButton = leavePanel.transform.FindDeep("NoButton", true).GetComponent<UIButton>();

            TurnPaneleffect = transform.FindDeep("TurnPanelTextPanel").GetComponent<UITweenMover>();
            ChatPanelEffect = transform.FindDeep("ChatCloud").GetComponent<UITweenMover>();

            autoRollDice = transform.FindDeep("DiceToggle").gameObject;
            UndoButton.onClick.AddListener(OnUndoActionClick);
            changeTheTurn.onClick.AddListener(OnChangeTurnClick);
            rolltheDice.onClick.AddListener(OnRollTheDiceClick);
            OpenChatMenu.onClick.AddListener(OnChatButtonClick);
            noButton.onClick.AddListener(HideLeavePanel);
            okButton.onClick.AddListener(LeaveTheGame);
            diceOn.onClick.AddListener(OnAutoRollDiceClick);
            diceOff.onClick.AddListener(OnAutoRollDiceClick);
            resignButton.onClick.AddListener(ShowLeavePanel);
            base.SetUIRefrences();
        }



        protected override void OnEnable()
        {
            base.OnEnable();
            if (simInstance != null)
            {
                simInstance.OnDiceRolled += OnDiceChanged;
                //simInstance.OnTableReady += Instance_OnTableReady;
                simInstance.OnGameDataReady += SimInstance_OnGameDataReady;
                simInstance.OnGameFinished += SimInstance_OnGameFinished;
                simInstance.OnReplayIsReady += SimInstance_OnReplayIsReady;
                simInstance.OnReplayEnd += SimInstance_OnReplayEnd;

            }

            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.OnSimpleChatRecived += Instance_OnSimpleChatRecived;
            }
            if (countDown == null)
            {
                countDown = AudioManager.Instance.Load("CountDown", AudioManager.SoundTypes.Effect);
                countDown.Stop();
                countDown.Volume = 100;
                countDown.AutoUnload = false;

            }

            if (chatRecivedAudio == null)
            {
                chatRecivedAudio = AudioManager.Instance.Load("ViewChange", AudioManager.SoundTypes.Effect);
                chatRecivedAudio.Stop();
                chatRecivedAudio.Volume = 100;
                chatRecivedAudio.AutoUnload = false;
            }
        }



        protected override void OnDisable()
        {
            base.OnDisable();
            if (simInstance != null)
            {
                simInstance.OnDiceRolled -= OnDiceChanged;
                //simInstance.OnTableReady -= Instance_OnTableReady;
                simInstance.OnGameDataReady -= SimInstance_OnGameDataReady;
                simInstance.OnGameFinished -= SimInstance_OnGameFinished;
                simInstance.OnReplayIsReady -= SimInstance_OnReplayIsReady;
                simInstance.OnReplayEnd -= SimInstance_OnReplayEnd;


            }


            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.OnSimpleChatRecived -= Instance_OnSimpleChatRecived;
            }


        }


        protected override void LateUpdate()
        {

            if (isReplay || !TableManager.Instance.IsGameStarted)
            {

                return;
            }


            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    MoveTurnFlag();
            //}

            //if (Input.GetKeyDown(KeyCode.O))
            //{
            //    ChatPanelEffect.OnAnimateInsideIn();
            //}

            //if (Input.GetKeyDown(KeyCode.I))
            //{
            //    ChatPanelEffect.OnAnimateInsideOut();
            //}
            UpdateFillBars();
            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                UndoButton.gameObject.SetActive(false);
                changeTheTurn.gameObject.SetActive(false);
                rolltheDice.gameObject.SetActive(false);


                return;
            }

            base.Update();


            switch (simInstance.YourColor)
            {
                case Simulation.Data.Game.PlayerColors.White:
                    moveCount = simInstance.CurrentSimulator.Frame.Board.WhitePlayer.MoveCount;
                    break;
                case Simulation.Data.Game.PlayerColors.Black:
                    moveCount = simInstance.CurrentSimulator.Frame.Board.BlackPlayer.MoveCount;
                    break;
                default:
                    break;
            }

            if (moveCount == 0 && simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length != 0)
            {
                OnChangeTurnClick();

                return;
            }

            rolltheDice.gameObject.SetActive(!isDiceRolled);
            UndoButton.gameObject.SetActive(simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length != simInstance.Board.TurnDice.Moves.Length);

            bool isTurnChange = simInstance.CurrentSimulator.Frame.Board.TurnDice.Moves.Length == 0;
            changeTheTurn.gameObject.SetActive(isTurnChange);
            if (isTurnChange)
                Dice.Instance.ResetDiceTweens();
        }

        private void SimInstance_OnGameDataReady(Simulation.Data.Game.PlayerColors Color)
        {
            RequestManagers.RequestManager.Instance.Network.OnConnectionLost += Network_OnConnectionLost;
            RequestManagers.RequestManager.Instance.Network.OnConnectionRestored += Network_OnConnectionRestored;
            RequestManagers.RequestManager.Instance.Network.OnRestoreSessionRespond += Network_OnRestoreSessionRespond;
            connectionIsPoor.gameObject.SetActive(false);
            Instance_OnTableReady();
        }

        private void Network_OnConnectionRestored()
        {
            try
            {
                if (TableManager.Instance.IsReplay)
                    return;
                RequestManagers.RequestManager.Instance.Network.RestoreSession();
            }
            catch (Exception e)
            {
                Debug.LogAssertion(e);
            }
        }

        private void SimInstance_OnReplayIsReady()
        {
            Instance_OnTableReady();
            isReplay = true;
            ufillBar.fillAmount = ofillBar.fillAmount = 0;
            OnRollTheDiceClick();
            OpenChatMenu.gameObject.SetActive(false);
            autoRollDice.gameObject.SetActive(false);
            rolltheDice.gameObject.SetActive(false);

        }


        private void Network_OnConnectionLost()
        {
            connectionIsPoor.gameObject.SetActive(true);
        }

        private void SimInstance_OnReplayEnd()
        {
            isReplay = false;
            OpenChatMenu.gameObject.SetActive(true);
        }

        private void Instance_OnTableReady()
        {
            isDiceRolled = false;
            isReplay = false;
            ufillBar.fillAmount = ofillBar.fillAmount = 1;
            autoRollDice.gameObject.SetActive(true);
            UndoButton.gameObject.SetActive(false);
            changeTheTurn.gameObject.SetActive(false);
            rolltheDice.gameObject.SetActive(false);
            leavePanel.gameObject.SetActive(false);
            SetRollVisualState();

            UIManager.Instance.HideUI("ChatMenu");
            // MoveTurnFlag();
            // turnText.text = simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor ? GameDataManager.GetString("YourTurn") : GameDataManager.GetString("OpponentTurn");
            uName.text = UserInfoManager.Instance.CurrentPlayer.UserName;
            uAvatar.sprite = GameResourceManager.Instance.LoadAvatarSprite(UserInfoManager.Instance.CurrentPlayer.AvatarID.ToString());
            uLevel.text = string.Format(GameDataManager.GetString("Level"), UserInfoManager.Instance.CurrentPlayer.Level);
            uPl.sprite = simInstance.YourColor == Simulation.Data.Game.PlayerColors.Black ? GameResourceManager.Instance.LoadSprite("FirstBoard/BlackBeed") : GameResourceManager.Instance.LoadSprite("FirstBoard/WhiteBeed");
            oName.text = UserInfoManager.Instance.Opponnent.UserName;
            oAvatar.sprite = GameResourceManager.Instance.LoadAvatarSprite(UserInfoManager.Instance.Opponnent.AvatarID.ToString());
            oLevel.text = string.Format(GameDataManager.GetString("Level"), UserInfoManager.Instance.Opponnent.Level);
            oPl.sprite = simInstance.YourColor == Simulation.Data.Game.PlayerColors.Black ? GameResourceManager.Instance.LoadSprite("FirstBoard/WhiteBeed") : GameResourceManager.Instance.LoadSprite("FirstBoard/BlackBeed");
            ResetFillBars();

            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor)
                OnRollTheDiceClick();
            else if (simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                if (IsAutoRoll)
                {
                    OnRollTheDiceClick();
                    rolltheDice.gameObject.SetActive(false);
                }
                else
                {
                    rolltheDice.gameObject.SetActive(true);
                }

            }
        }



        private void Network_OnRestoreSessionRespond(SessionRestoreResults Result)
        {
            switch (Result)
            {
                case SessionRestoreResults.Done:

                    connectionIsPoor.gameObject.SetActive(false);
                    break;
                case SessionRestoreResults.Failed:
                    break;
                default:
                    break;
            }
        }


        private void SimInstance_OnGameFinished(Simulation.Data.Game.PlayerColors WinnerColor, GameFinishReasons Reason, int Score)
        {
            countDown.Stop();
            leavePanel.gameObject.SetActive(false);
            UIManager.Instance.HideUI("ChatMenu");
            RequestManagers.RequestManager.Instance.Network.OnConnectionLost -= Network_OnConnectionLost;
            RequestManagers.RequestManager.Instance.Network.OnRestoreSessionRespond -= Network_OnRestoreSessionRespond;
            RequestManagers.RequestManager.Instance.Network.OnConnectionRestored -= Network_OnConnectionRestored;

        }


        private void LeaveTheGame()
        {
            RequestManagers.RequestManager.Instance.Resign();
        }


        private void HideLeavePanel()
        {
            leavePanel.gameObject.SetActive(false);
        }

        private void ShowLeavePanel()
        {
            if (isReplay)
            {
                simInstance.FinishCurrentReplay();
            }
            else
            {
                UIManager.Instance.HideUI("ChatMenu");
                leavePanel.gameObject.SetActive(true);
            }
        }

        private void OnChatButtonClick()
        {
            leavePanel.gameObject.SetActive(false);
            UIManager.Instance.ShowUI("ChatMenu");
        }


        private void SetDiceState()
        {
            IsAutoRoll = !IsAutoRoll;
        }

        private void SetRollVisualState()
        {
            diceOff.gameObject.SetActive(!IsAutoRoll);
            diceOn.gameObject.SetActive(IsAutoRoll);
        }

        private void OnAutoRollDiceClick()
        {
            SetDiceState();
            SetRollVisualState();
        }
        private void Instance_OnSimpleChatRecived(int PackID, int Index)
        {
            chatText.text = string.Empty;
            string chat = string.Empty;

            for (int i = 0; i < ChatManager.Instance.SimpleChatList.Length; ++i)
            {
                ChatPack ch = ChatManager.Instance.SimpleChatList[i];
                if (ch.ID != PackID)
                    continue;
                chat = GameDataManager.GetString(ch.Chat[Index].Content);
            }

            if (chat == string.Empty)
                return;
            ChatPanelEffect.gameObject.SetActive(true);

            ChatPanelEffect.OnAnimateInsideIn(() => SetText(chat));
        }

        private void SetText(string Text)
        {
            chatRecivedAudio.Stop();
            chatRecivedAudio.Play();
            chatText.text = Text;
            ScheduleManager.Instance.AddSchedule(() =>
            {
                ChatPanelEffect.OnAnimateInsideOut();
                ChatPanelEffect.gameObject.SetActive(false);
            }, 5);
        }




        private void OnDiceChanged()
        {
            Debug.Log("OnDiceChanged");
            isDiceRolled = false;
            // turnText.text = simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor ? GameDataManager.GetString("YourTurn") : GameDataManager.GetString("OpponentTurn");
            //  MoveTurnFlag();
            ResetFillBars();

            // Debug.LogError(simInstance.CurrentSimulator.Frame.Board.TurnColor + " == " +simInstance.YourColor);
            if (simInstance.YourColor != simInstance.CurrentSimulator.Frame.Board.TurnColor || IsAutoRoll || isReplay)
            {
                OnRollTheDiceClick();
            }

        }

        private void OnRollTheDiceClick()
        {
            Dice.Instance.RollTheDice(NoMoveExist);

            if (simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor)
                isDiceRolled = true;
        }


        private void NoMoveExist()
        {
            if (!TableManager.Instance.IsGameStarted)
                return;

            if (simInstance.YourColor == simInstance.Board.TurnColor)
            {
                //isDiceRolled = true;
                switch (simInstance.YourColor)
                {
                    case Simulation.Data.Game.PlayerColors.White:
                        {
                            if (simInstance.CurrentSimulator.Frame.Board.WhitePlayer.MoveCount == 0)
                            {

                                OnChangeTurnClick();
                                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("YouCannotMove"));
                            }
                        }
                        break;
                    case Simulation.Data.Game.PlayerColors.Black:
                        {
                            if (simInstance.CurrentSimulator.Frame.Board.BlackPlayer.MoveCount == 0)
                            {

                                OnChangeTurnClick();
                                PopupTextMenu.Instance.ShowPopUpText(GameDataManager.GetString("YouCannotMove"));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void MoveTurnFlag()
        {
            if (LeanTween.isTweening(TurnPaneleffect.RectTransformPanel.gameObject))
                TurnPaneleffect.CancelTween();

            if (simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor)
            {
                TurnPaneleffect.OnAnimateInsideOut();
                // simInstance.YourColor = Simulation.Data.Game.PlayerColors.Black;
            }
            else
            {
                TurnPaneleffect.OnAnimateInsideIn();
                //  simInstance.YourColor = Simulation.Data.Game.PlayerColors.White;
            }

        }

        private void ResetFillBars()
        {
            if (!isReplay)
            {
                countDown.Stop();

                period = TableManager.Instance.SelectedTable.TurnTime;
                timeInterval = period - 1;
            }
            else
            {
                ufillBar.fillAmount = ofillBar.fillAmount = 0;
            }
        }


        private void UpdateFillBars()
        {
            if (!TableManager.Instance.IsGameStarted)
                return;
            period -= Time.deltaTime;

            if (period > timeInterval)
                return;

            float time = period / TableManager.Instance.SelectedTable.TurnTime;
            if (time <= 0)
            {
                countDown.Stop();
            }
            else if (simInstance.YourColor == simInstance.CurrentSimulator.Frame.Board.TurnColor && time < 0.15F && !countDown.AlreadyPlayed)
            {
                countDown.Stop();
                countDown.Play();
            }

            timeInterval = period - 0.1F;
            if (simInstance.CurrentSimulator.Frame.Board.TurnColor == simInstance.YourColor)
            {
                ufillBar.fillAmount = Mathf.Lerp(ufillBar.fillAmount, time, 0.1F);
                //  LeanTween.value(ufillBar.fillAmount ,  period / TableManager.Instance.SelectedTable.TurnTime,0.5f).setOnUpdate(updateUFillBar);

            }
            else
            {
                ofillBar.fillAmount = Mathf.Lerp(ofillBar.fillAmount, time, 0.1F);

                //   LeanTween.value(ofillBar.fillAmount, period / TableManager.Instance.SelectedTable.TurnTime,0.5f).setOnUpdate(updateOFillBar);
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