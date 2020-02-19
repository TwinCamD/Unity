using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOpeningSphere : MonoBehaviour {

    [SerializeField] private float _connectTime = 5f;
    [SerializeField] private float _makeCallTime = 10f;
    [SerializeField] private float _destroyTime = 28f;
    [SerializeField] private GameObject _switchStereoObj;
    private SwitchStereoMono _switchStereoMono;
    public GameObject l, r;
    private SkywayVideoReciever _skywayVideoRecieverL;
    private SkywayVideoReciever _skywayVideoRecieverR;
    public GameObject lb, rb;
    private SkywayDataConnect _skywayDataConnectL;
    private SkywayDataConnect _skywayDataConnectR;

    // Use this for initialization
    void Start () {
	    _switchStereoMono = _switchStereoObj.GetComponent<SwitchStereoMono>();
        _switchStereoMono.ChangeTargetEye(SwitchStereoMono.TargetEye.Left);

        _skywayVideoRecieverL = l.GetComponent<SkywayVideoReciever>();
        _skywayVideoRecieverR = r.GetComponent<SkywayVideoReciever>();

        _skywayDataConnectL = lb.GetComponent<SkywayDataConnect>();
        _skywayDataConnectR = rb.GetComponent<SkywayDataConnect>();

        Invoke("Connect", _connectTime);
        Invoke("MakeCall", _makeCallTime);
	    Invoke("DelayMethod", _destroyTime);
    }
	
	// Update is called once per frame
	void Update () {
	}

    void Connect() {
        _skywayDataConnectL.Connect();
        _skywayDataConnectR.Connect();
    }

    void MakeCall() {
        _skywayVideoRecieverL.MakeCall();
        _skywayVideoRecieverR.MakeCall();
    }

    void DelayMethod() {
        Destroy(this.gameObject);
        _switchStereoMono.ChangeTargetEye(SwitchStereoMono.TargetEye.Both);
    }
}
