﻿using System.Collections;
using Effect;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * 该类管理了整个游戏的逻辑和UI(也可以搞个负责UI的UIManager)
 * 挂在ScriptHolder上(没有像我之前那样挂在bg或者canvas上)
 * 如：
 * 	点击换枪操作，开火等
 **/
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    //UI组件，通过拖拽赋值的方式(命名查找的方式也可以)
    public Text oneShootCostText; //当前每炮需要花费多少金币
    public Text goldText;
    public Text lvText;
    public Text lvNameText;
    public Text smallCountdownText; //小的倒计时
    public Text bigCountdownText; //大的倒计时
    public Button bigCountdownButton;
    public Button backButton;
    public Button settingButton;
    public Slider expSlider;

    private const int BigCountdown = 240; //每4分钟发一次奖励，inspector中的属性优先级大于这里的属性
    private const int SmallCountdown = 60; //每分钟小的奖励

    public int lv; //lv 等级
    public int exp;
    public int gold = 500;

    public float bigTimer = BigCountdown;
    public float smallTimer = SmallCountdown;
    public Color goldColor;
    public int bgIndex = 0; //当前背景的Index

    public Image bgImage; //背景

    public GameObject lvUpTips; //升级提示

    //各种粒子特效,这些特效只需要实例化出来就行，隐藏交给Ef_DestroySelf
    public GameObject seaWaveEffect;
    public GameObject fireEffect;
    public GameObject changeEffect;
    public GameObject lvEffect;
    public GameObject goldEffect;

    public Transform bulletHolder; // 子弹的容器
    public Sprite[] bgSprites; // 背景图片的精灵
    public GameObject[] gunGos; // 所有的枪的对象

    public GameObject[] bullet1Gos; // 1号枪的子弹数组
    public GameObject[] bullet2Gos; //...
    public GameObject[] bullet3Gos;
    public GameObject[] bullet4Gos;
    public GameObject[] bullet5Gos;

    // 当前使用的oneShootCosts的序号， costIndex/4:表示当前是第几档的炮
    private int _costIndex;

    // 每一炮所需的金币数和造成的伤害值，每4个属于一个档次：0，1，2，3，4
    private readonly int[] _oneShootCosts =
        {5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000};

    private readonly string[] _lvName = {"新手", "入门", "钢铁", "青铜", "白银", "黄金", "白金", "钻石", "大师", "宗师"};

    protected void Awake()
    {
        Instance = this;
    }

    protected void Start()
    {
        // 使用PlayerPrefs存储用户数据
        gold = PlayerPrefs.GetInt("gold", gold);
        lv = PlayerPrefs.GetInt("lv", lv);
        exp = PlayerPrefs.GetInt("exp", exp);
        smallTimer = PlayerPrefs.GetFloat("scd", SmallCountdown);
        bigTimer = PlayerPrefs.GetFloat("bcd", BigCountdown);
        UpdateUI();
    }

    protected void Update()
    {
        ChangeBulletCost(); //每帧检测滚轮调炮的威力
        Fire(); //每帧检测是否开火了
        UpdateUI(); //更新UI
        ChangeBg();
    }

    private void ChangeBg()
    {
        if (bgIndex == lv / 20) return;

        // 每过20关会换一次bg
        bgIndex = lv / 20;
        AudioManager.Instance.PlayEffectSound(AudioManager.Instance.seaWaveClip);
        Instantiate(seaWaveEffect); // 播放seaWave特效

        bgImage.sprite = bgIndex >= 3 ? bgSprites[3] : bgSprites[bgIndex];
    }

    private void UpdateUI()
    {
        bigTimer -= Time.deltaTime;
        smallTimer -= Time.deltaTime;
        if (smallTimer <= 0)
        {
            // 每分钟奖励50个金币
            smallTimer = SmallCountdown;
            gold += 50;
        }

        if (bigTimer <= 0 && bigCountdownButton.gameObject.activeSelf == false)
        {
            // 每4分钟给一次大的奖励
            bigCountdownText.gameObject.SetActive(false);
            bigCountdownButton.gameObject.SetActive(true);
        }

        // 经验等级换算公式：升级所需经验=1000+200*当前等级
        while (exp >= 1000 + 200 * lv)
        {
            exp = exp - (1000 + 200 * lv);
            lv++;
            lvUpTips.SetActive(true); // 显示升级提示
            lvUpTips.transform.Find("Text").GetComponent<Text>().text = lv.ToString();
            StartCoroutine(lvUpTips.GetComponent<Ef_HideSelf>().HideSelf(0.6f)); // 0.6s后自动隐藏
            AudioManager.Instance.PlayEffectSound(AudioManager.Instance.lvUpClip);
            Instantiate(lvEffect); // 显示升级特效
        }

        goldText.text = "$" + gold;
        lvText.text = lv.ToString();

        // 段位
        lvNameText.text = (lv / 10) <= 9 ? _lvName[lv / 10] : _lvName[9];

        smallCountdownText.text = "  " + (int) smallTimer / 10 + "  " + (int) smallTimer % 10;
        bigCountdownText.text = (int) bigTimer + "s";
        expSlider.value = ((float) exp) / (1000 + 200 * lv);
    }

    private void Fire()
    {
        var useBullets = bullet5Gos; // 当前使用的炮的型号

        // 点击了鼠标左键， 并且没有点击到UI上
        // 此处需要把背景的RaycastTarget去掉,否则IsPointerOverGameObject始终 = true
        if (!Input.GetMouseButtonDown(0) || EventSystem.current.IsPointerOverGameObject()) return;

        if (gold - _oneShootCosts[_costIndex] >= 0)
        {
            // costIndex / 4 : 炮的型号
            switch (_costIndex / 4)
            {
                case 0:
                    useBullets = bullet1Gos;
                    break;
                case 1:
                    useBullets = bullet2Gos;
                    break;
                case 2:
                    useBullets = bullet3Gos;
                    break;
                case 3:
                    useBullets = bullet4Gos;
                    break;
                case 4:
                    useBullets = bullet5Gos;
                    break;
            }

            var bulletIndex = (lv % 10 >= 9) ? 9 : lv % 10; // 当前型号的炮的哪种颜色的子弹序号
            gold -= _oneShootCosts[_costIndex]; // 开炮后更新gold
            AudioManager.Instance.PlayEffectSound(AudioManager.Instance.fireClip); // 开火的音效
            Instantiate(fireEffect); // 开火的特效

            // 实例化子弹(子弹飞的事情，撞到鱼生成网，多久消失，撞到border消失都交给子弹类自己搞定)
            var bullet = Instantiate(useBullets[bulletIndex]);
            bullet.transform.SetParent(bulletHolder, false);
            bullet.transform.position =
                gunGos[_costIndex / 4].transform.Find("FirePos").transform.position; // 子弹的位置为每种炮中子弹的生成位置
            bullet.transform.rotation =
                gunGos[_costIndex / 4].transform.Find("FirePos").transform.rotation; // 子弹的方向(旋转角度)为炮的旋转角度
            bullet.GetComponent<BulletAttr>().damage = _oneShootCosts[_costIndex]; // 子弹的伤害值 = 子弹的cost
            // bullet挂上Ef_AutoMove脚本，动态挂还是静态挂一样
            bullet.AddComponent<Ef_AutoMove>().dir = Vector3.up; // 子弹的x轴正方向没有处理，故子弹的dir = Vector3.top
            bullet.GetComponent<Ef_AutoMove>().speed = bullet.GetComponent<BulletAttr>().speed; // 设置子弹的speed
        }
        else
        {
            StartCoroutine(GoldNotEnough());
        }
    }

    private void ChangeBulletCost()
    {
        // 按鼠标滚轮调炮的威力
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            OnButtonMDown();
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            OnButtonPDown();
        }
    }

    /**
     * 调大枪的威力
     **/
    public void OnButtonPDown()
    {
        gunGos[_costIndex / 4].SetActive(false); // 当前档次枪隐藏，costIndex / 4表示枪的档次，一共有4档
        _costIndex++; // 下一档

        AudioManager.Instance.PlayEffectSound(AudioManager.Instance.changeClip); // 换枪音效
        Instantiate(changeEffect); // 换枪特效

        _costIndex = (_costIndex > _oneShootCosts.Length - 1) ? 0 : _costIndex;
        gunGos[_costIndex / 4].SetActive(true); // 显示下一个档次的枪
        oneShootCostText.text = "$" + _oneShootCosts[_costIndex]; // 更新ui,当前枪每炮的cost
    }

    /**
     * 调小枪的威力
     **/
    public void OnButtonMDown()
    {
        gunGos[_costIndex / 4].SetActive(false);
        _costIndex--;
        AudioManager.Instance.PlayEffectSound(AudioManager.Instance.changeClip);
        Instantiate(changeEffect);
        _costIndex = (_costIndex < 0) ? _oneShootCosts.Length - 1 : _costIndex;
        gunGos[_costIndex / 4].SetActive(true);
        oneShootCostText.text = "$" + _oneShootCosts[_costIndex];
    }

    // 每4分钟获得大的奖励，加500金币
    public void OnBigCountdownButtonDown()
    {
        gold += 500;
        AudioManager.Instance.PlayEffectSound(AudioManager.Instance.rewardClip);
        Instantiate(goldEffect);

        bigCountdownButton.gameObject.SetActive(false);
        bigCountdownText.gameObject.SetActive(true);
        bigTimer = BigCountdown; // 重置计时
    }

    // 金币不够，goldText闪动效果，可以用动画来做(更容易)
    private IEnumerator GoldNotEnough()
    {
        goldText.color = goldColor;
        goldText.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        goldText.color = goldColor;
    }
}