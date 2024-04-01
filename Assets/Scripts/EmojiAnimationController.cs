using System;
using DG.Tweening;
using UnityEngine;

public class EmojiAnimationController : SingletonComponent<EmojiAnimationController>
{
    [SerializeField]private SpriteRenderer spriteRenderer;

    private readonly Vector3 _wordRot = new Vector3(64,0, 0);
    private static readonly int Play = Animator.StringToHash("Play");

    private void Start()
    {
        transform.eulerAngles = _wordRot;
    }

    private void Update()
    {
        if(transform.eulerAngles != _wordRot)
            transform.eulerAngles = _wordRot;
    }

    public void Up_Animation(Sprite sprite,Vector3 position)
    {
        position += new Vector3(0, 30, 0);
        transform.position = position;

        spriteRenderer.sprite = sprite;
        
        transform.localScale = Vector3.zero;

        var sequence = DOTween.Sequence();
        
        transform.eulerAngles = _wordRot;

        sequence.Append(transform.DOScale(Vector3.one*10, 0.4f)).SetEase(Ease.OutBack);
        sequence.AppendInterval(2f);
        sequence.Append(spriteRenderer.DOFade(0, 1f)).OnComplete(() => Destroy(gameObject));
        
    }

    public void Set_Horn_Blink_Animation()
    {
        GetComponent<SpriteRenderer>().DOFade(0f, 0.5f).SetDelay(0.6f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}