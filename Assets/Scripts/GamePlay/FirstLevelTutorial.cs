using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Manager;
using UnityEngine;

namespace GamePlay
{
    public class FirstLevelTutorial : SingletonComponent<FirstLevelTutorial>
    {
        private List<SwipeDirection> _swipeDirections = new List<SwipeDirection>
            { SwipeDirection.Left, SwipeDirection.Right, SwipeDirection.Up };
      
        public int carNumber = 0;
        
        private TweenerCore<Vector3, Vector3, VectorOptions> _moveAnim = null;
        private GameObject _hand = null;

        private void Start()
        {
            var rot = GameManager.Instance.tutorialHand.transform.eulerAngles;

            var startPos = FindObjectsOfType<VehicleController>().ToList().Find(x=>x.name == carNumber.ToString()).GetHandPos();
            
            _hand = Instantiate(GameManager.Instance.tutorialHand, startPos, Quaternion.Euler(rot),
                GameManager.Instance.transform);
            
            HandAnimation(startPos);
        }

        private void HandAnimation(Vector3 startPos)
        {
            _moveAnim.Kill();
            _hand.transform.position = startPos;
            var targetPos = GetTargetPos(GetTargetPos(startPos));
            _moveAnim = _hand.transform.DOMove(targetPos, 1f).SetDelay(0.5f).SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        private Vector3 GetTargetPos(Vector3 currentPos)
        {
            var swipeDir = _swipeDirections[carNumber];
            const int amount = 10;
            return swipeDir switch
            {
                SwipeDirection.Left => currentPos - new Vector3(amount, 0, 0),
                SwipeDirection.Right => currentPos + new Vector3(amount, 0, 0),
                SwipeDirection.Up => currentPos + new Vector3(0, 0, amount - 5),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public SwipeDirection GetCarSwipe()
        {
            return _swipeDirections[carNumber];
        }

        public void IncreaseCarNumber()
        {
            if (carNumber < 2)
            {
                carNumber++;
                var startPos = FindObjectsOfType<VehicleController>().ToList().Find(x=>x.name == carNumber.ToString()).GetHandPos();
                HandAnimation(startPos);
            }
            else
            {
                Destroy(_hand);
            }
        }
    }
}