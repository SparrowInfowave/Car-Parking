using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay
{
    public class MoveAndTimeController : MonoBehaviour
    {
        [SerializeField] private Text moveText; 
        [SerializeField] private Text timerText;
        [SerializeField] private Slider timeSlider;
        
        private bool _isDecreaseTime = false;
        
        private Tween _sliderAnim = null;


        private void OnEnable()
        {
            _isDecreaseTime = false;
        }

        public void Set_TargetMoves(int moves)
        {
            GameManager.Instance.targetMoves = moves;
            Set_MoveText();
        }
        public void Set_TimerText(int time)
        {
            GameManager.Instance.targetTime = time;
            Set_TimeText();
        }
    
        private void Set_MoveText()
        {
            moveText.text = GameManager.Instance.targetMoves + " Moves";
        }
        private void Set_TimeText()
        {
            if(timerText == null)return;
            timerText.text = GameManager.Instance.Int_To_Minute_Second_Time(GameManager.Instance.targetTime);
        }
        
        public void StartDecrease_Time()
        {
            if (!_isDecreaseTime)
                Start_CountDown();
        }

        public void Decrease_Move()
        {
            if (GameManager.Instance.targetMoves > 0)
            {
                GameManager.Instance.targetMoves--;
                Set_MoveText();
            }

            if (GameManager.Instance.targetMoves <= 0)
            {
                GamePlayController.Instance.Start_For_OutOfMove_GameOver();
            }
            
        }
    
        public void Decrease_Time()
        {
            if (GameManager.Instance.targetTime > 0)
            {
                GameManager.Instance.targetTime--;
                Set_TimeText();
            }
        
            if (GameManager.Instance.targetTime <= 0)
            {
                GameManager.Instance.StartCoroutine(GameManager.Instance.GameOver(0.1f,GameOverReason.OutOfTime));
            }
        }

        public void Stop_CountDown()
        {
            _isDecreaseTime = false;
            _sliderAnim.Pause();
            CancelInvoke(nameof(Decrease_Time));
        }

        private void Start_CountDown()
        {
            _isDecreaseTime = true;
            InvokeRepeating(nameof(Decrease_Time), 0f, 1f);

            if (_sliderAnim == null)
                _sliderAnim = timeSlider.DOValue(0, GameManager.Instance.targetTime).SetEase(Ease.Linear);
            else
                _sliderAnim.Play();
        }

        public void Set_Slider()
        {
            timeSlider.maxValue = GameManager.Instance.targetTime;
            timeSlider.minValue = 0;
            timeSlider.value = timeSlider.maxValue;
        }

        private void OnDisable()
        {
            _isDecreaseTime = false;
        }
    }
}
