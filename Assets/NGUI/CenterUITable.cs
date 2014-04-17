using UnityEngine;
using System.Collections;

public class CenterUITable : MonoBehaviour 
{
    public bool centerOnX = true;
    public bool centerOnY = false;
    public GameObject uiTableGameObject;
    
    private UITable _uiTable;
    
    void Start () 
    {
        // find UITable on this gameObject if not explicitly set
        if (uiTableGameObject == null)
            uiTableGameObject = this.gameObject;
        
        _uiTable = uiTableGameObject.GetComponent<UITable>();
        
        if (_uiTable == null)
        {
            Debug.LogWarning("uiTableGameObject does not have a UITable Component at Start() time!  Destroying.");
            Destroy(this);
            return;
        }
        
        _uiTable.onReposition += Reposition;
    }
    
    void Reposition()
    {
        // do the actual repositioning
        Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(_uiTable.transform);
        Vector3 pos = _uiTable.transform.localPosition;
        
        if (centerOnX)
            pos.x = -bounds.center.x;
        
        if (centerOnY)
            pos.y = -bounds.center.y;
        
        _uiTable.transform.localPosition = pos;
    }
    
    void OnDestroy()
    {
        if (_uiTable)
            _uiTable.onReposition -= Reposition;
    }
}