using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace GamePlay
{
    public class BarricadeArmMovement : SingletonComponent<BarricadeArmMovement>
    {
        private readonly float _time = 0.5f;

        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _open = null;
        private TweenerCore<Quaternion, Vector3, QuaternionOptions> _close = null;

        public void OpenBarricade()
        {
            _close?.Kill();
            _open = transform.DOLocalRotate(new Vector3(-60, 0, 0), _time).SetEase(Ease.OutSine);
        }
    
        public void Close_Barricade()
        {
            if (_open.active) return;
            _close = transform.DOLocalRotate(new Vector3(0, 0, 0), _time).SetEase(Ease.OutSine);
        }
    }
}