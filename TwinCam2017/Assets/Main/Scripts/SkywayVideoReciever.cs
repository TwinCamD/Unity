/*Summary
 *  twincam-recieverをunity側から動かすためのスクリプト
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ZenFulcrum.EmbeddedBrowser;

[RequireComponent(typeof(Browser))]
public class SkywayVideoReciever : MonoBehaviour {

    private Browser _browser;

    public string YourId = "user";
    public string CallToId = "tc";

    void Awake() {
        _browser = GetComponent<Browser>();
    }
    // Use this for initialization
    void Start () {
        GetPeerId();
    }
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.M)) {
	        MakeCall();
	    }
	    if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.E)) {
	        EndCall();
	    }
    }

    void FixedUpdate() {
        eventResult();
    }

    private void eventResult() {
        _browser.RegisterFunction("Open", result => Debug.Log("Opened"));
        _browser.RegisterFunction("Error", result => {
            string err = result[0];
            Debug.LogError(err);
        });
        _browser.RegisterFunction("Close", result => Debug.LogWarning("closed"));
        _browser.RegisterFunction("Disconnected", result => Debug.LogWarning("Disconnected"));
    }

    //[ContextMenu("GetPeerId")]
    public void GetPeerId() {
        _browser.CallFunction("GetPeerId", YourId);
    }

    [ContextMenu("MakeCall")]
    public void MakeCall() {
        _browser.CallFunction("MakeCall", CallToId);
    }

    [ContextMenu("EndCall")]
    public void EndCall() {
        _browser.CallFunction("EndCall");
    }

}
