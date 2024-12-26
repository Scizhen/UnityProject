using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VisualSpline;

public class LayoutSystemControl : MonoBehaviour
{
    //[Header("Basic Buttons")]
    //public GameObject MainMenuButoons;

    [Header("PSO")]
    public Button CreatePSOButton;
    public List<GameObject> machineGameObject = new List<GameObject>();
    public GameObject BackgroundPlane;
    private PSO_FunctionTest PSOFuntion = new PSO_FunctionTest();

    [Header("GEN")]
    public Button CreateGENButton;
    public Button CreateGENEncodeButton;
    public GEN_Encoed_result GENEncoedFuntion;
    public GameObject GENResultChart;
    public GameObject PieceLineChart;
    public GEN_FunctionTest GENFuntion;

    [Header("Transform")]
    public GameObject TransformAnalys;

    //获取布局仿真的UI组件
    private RectTransform PSO_ViewCanvas;
    private RectTransform PSO_Basic;
    private RectTransform PSO_SLP;
    private RectTransform PSO;
    private RectTransform PSO_GA ;
    private RectTransform PSO_Prefeb;

    //获取调度仿真的UI组件
    private RectTransform GEN_ViewCanvas;
    private RectTransform GEN_Basic;
    private RectTransform GEN_Simulation;

    //获取动线分析的UI组件
    private RectTransform Tra_ViewCanvas;
    private RectTransform Tra_Basic;

    //开始布局仿真
    void CreatePSOFuntion()
    {
        //传递参数
        PSO_BasicParameterTransfer(PSO_Basic);
        PSO_SLPParameterTransfer(PSO_SLP);
        PSOParameterTransfer(PSO);
        PSO_GAParameterTransfer(PSO_GA);
        PSO_PrefebParameterTransfer(PSO_Prefeb);
        //开启布局
        BackgroundPlane.GetComponent<Transform>().localScale = new Vector3((float)PSOFuntion.xlimit[1], 0.01f, (float)PSOFuntion.ylimit[1]);
        BackgroundPlane.GetComponent<Transform>().localPosition = new Vector3((float)PSOFuntion.xlimit[1]/2, 0.01f, (float)PSOFuntion.ylimit[1]/2);
        PSOFuntion.StartPSOFunction();
    }


    void CreateGENFuntion()
    {
        GEN_BasicParameterTransfer(GEN_Basic);
        //开启布局
        GENFuntion.StartGENFunction();
    }
    void CreateGENEncodeFuntion()
    {
        GENEncoedFuntion.gameObject.SetActive(true);
        GENEncoedFuntion.StartGENEncoed();
    }
    void GENSimulationParameter(RectTransform Rect)
    {
        GENEncoedFuntion.startWork = Rect.Find("startWork").Find("Toggle").GetComponent<Toggle>().isOn;
        if (Rect.Find("timeSpeed").Find("input").GetComponent<TMP_InputField>().text != "")
            GENEncoedFuntion.timeSpeed = float.Parse(Rect.Find("timeSpeed").Find("input").GetComponent<TMP_InputField>().text);
    }
    void TransformParameter(RectTransform Rect)
    {
        if (Rect.Find("LineWidth").Find("input").GetComponent<TMP_InputField>().text != "")
            GENEncoedFuntion.LineWidth = float.Parse(Rect.Find("LineWidth").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("LineWidthRandom").Find("input").GetComponent<TMP_InputField>().text != "")
            GENEncoedFuntion.LineWidthRandom = float.Parse(Rect.Find("LineWidthRandom").Find("input").GetComponent<TMP_InputField>().text);

    }
    void GEN_BasicParameterTransfer(RectTransform Rect)
    {
        if (Rect.Find("iteration").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.iteration = int.Parse(Rect.Find("iteration").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("gensize").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.gensize = int.Parse(Rect.Find("gensize").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("workpieceNumber").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.workpieceNumber = int.Parse(Rect.Find("workpieceNumber").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("machineNum").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.machineNum = int.Parse(Rect.Find("machineNum").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("AGVNum").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.AGVNum = int.Parse(Rect.Find("AGVNum").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("processNumTotal").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.processNumTotal = int.Parse(Rect.Find("processNumTotal").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("CrossRange").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.CrossRange = double.Parse(Rect.Find("CrossRange").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("MutationProbability").Find("input").GetComponent<TMP_InputField>().text != "")
            GENFuntion.MutationProbability = double.Parse(Rect.Find("MutationProbability").Find("input").GetComponent<TMP_InputField>().text);

    }
    void PSO_BasicParameterTransfer(RectTransform Rect)
    {
        if (Rect.Find("xlimit").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.xlimit = new double[2] { 0, double.Parse(Rect.Find("xlimit").Find("input").GetComponent<TMP_InputField>().text) };
        if (Rect.Find("ylimit").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.ylimit = new double[2] { 0, double.Parse(Rect.Find("ylimit").Find("input").GetComponent<TMP_InputField>().text) };
        if (Rect.Find("equipmentNumber").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.equipmentNumber = int.Parse(Rect.Find("equipmentNumber").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("experimentNumber").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.experimentNumber = int.Parse(Rect.Find("experimentNumber").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("DataExportPath").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.DataExportPath = Rect.Find("DataExportPath").Find("input").GetComponent<TMP_InputField>().text;
        if (Rect.Find("DataExportName").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.DataExportName = Rect.Find("DataExportName").Find("input").GetComponent<TMP_InputField>().text;
        PSOFuntion.OpenLspInit = Rect.Find("OpenLspInit").Find("Toggle").GetComponent<Toggle>().isOn;
        PSOFuntion.OpenGeneral = Rect.Find("OpenGeneral").Find("Toggle").GetComponent<Toggle>().isOn;
        PSOFuntion.SaveEveryGenerationData = Rect.Find("SaveEveryGenerationData").Find("Toggle").GetComponent<Toggle>().isOn;
        PSOFuntion.SaveEveryPopsizeData = Rect.Find("SaveEveryPopsizeData").Find("Toggle").GetComponent<Toggle>().isOn;

    }
    void PSO_SLPParameterTransfer(RectTransform Rect)
    {
        if (Rect.Find("Lsp_Init").Find("input").GetComponent<TMP_InputField>().text != "")
        {
            string lspinit = Rect.Find("Lsp_Init").Find("input").GetComponent<TMP_InputField>().text;
            string[] machine = lspinit.Split(',');
            for (int i = 0; i < machine.Length; i++)
            {
                PSOFuntion.Lsp_Init[i] = double.Parse(machine[i]);
                Debug.Log(double.Parse(machine[i]));
            }
        }

    }
    void PSOParameterTransfer(RectTransform Rect)
    {
        if (Rect.Find("generation").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.generation = int.Parse(Rect.Find("generation").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("popsize").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.popsize = int.Parse(Rect.Find("popsize").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("Wmax").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.Wmax = double.Parse(Rect.Find("Wmax").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("Wmin").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.Wmin = double.Parse(Rect.Find("Wmin").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("C1").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.C1 = double.Parse(Rect.Find("C1").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("C2").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.C2 = double.Parse(Rect.Find("C2").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("T1").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.T1 = double.Parse(Rect.Find("T1").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("T2").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.T2 = double.Parse(Rect.Find("T2").Find("input").GetComponent<TMP_InputField>().text);

    }
    void PSO_GAParameterTransfer(RectTransform Rect)
    {
        if (Rect.Find("gen_size").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.gen_size = int.Parse(Rect.Find("gen_size").Find("input").GetComponent<TMP_InputField>().text);
        if (Rect.Find("Variation_Pro").Find("input").GetComponent<TMP_InputField>().text != "")
            PSOFuntion.Variation_Pro = double.Parse(Rect.Find("Variation_Pro").Find("input").GetComponent<TMP_InputField>().text);

    }
    void PSO_PrefebParameterTransfer(RectTransform Rect)
    {
        //运输通畅性预设
        bool transform = Rect.Find("transform").Find("Toggle").GetComponent<Toggle>().isOn;
        if (transform)
        {
            PSOFuntion.T1 = 20;
        }
        //空间利用率高预设
        bool space = Rect.Find("space").Find("Toggle").GetComponent<Toggle>().isOn;
        if (space)
        {
            PSOFuntion.T1 = 5;
            PSOFuntion.T2 = 100;
        }
        //作业关系合理性预设
        bool work = Rect.Find("work").Find("Toggle").GetComponent<Toggle>().isOn;
        if (work)
        {
            PSOFuntion.OpenGeneral = false;
            PSOFuntion.C1 = 1;
            PSOFuntion.C2 = 1;
            PSOFuntion.vlimit = new double[2] { -1, 1 };
        }
        //多工艺路线规划
        bool more = Rect.Find("more").Find("Toggle").GetComponent<Toggle>().isOn;
        if (more)
        {
            PSOFuntion.OpenGeneral = false;
        }
        //环境舒适性
        bool enviroment = Rect.Find("enviroment").Find("Toggle").GetComponent<Toggle>().isOn;
        if (enviroment)
        {
            PSOFuntion.T1 = 50;
            PSOFuntion.T2 = 100;
        }
    }
    //递归查找所有符合名字要求的对象
    List<GameObject> FindAllChildObjectsByName(GameObject parent, string name)
    {
        List<GameObject> foundObjects = new List<GameObject>();

        if (parent == null) return foundObjects;

        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
            {
                foundObjects.Add(child.gameObject);
            }

            // 递归查找子对象
            foundObjects.AddRange(FindAllChildObjectsByName(child.gameObject, name));
        }

        return foundObjects;
    }
    void SwitchingInterface(Button button)
    {
        Transform parent = button.transform.parent.parent;
        Transform view = parent.Find("View");
        //List<GameObject> viewChildren = new List<GameObject>();
        foreach (Transform child in view.transform)
        {
            child.gameObject.SetActive(false);
        }
        view.Find(button.name).gameObject.SetActive(true);

    }
// Start is called before the first frame update
void Awake()
    {

        //布局相关
        //控件初始化挂载
        PSO_ViewCanvas = CreatePSOButton.transform.parent.Find("View").GetComponent<RectTransform>();
        //获取对应UI组件
        PSO_Basic = PSO_ViewCanvas.Find("Basic").GetComponent<RectTransform>();
        PSO_SLP = PSO_ViewCanvas.Find("SLP").GetComponent<RectTransform>();
        PSO = PSO_ViewCanvas.Find("PSO").GetComponent<RectTransform>();
        PSO_GA = PSO_ViewCanvas.Find("GA").GetComponent<RectTransform>();
        PSO_Prefeb = PSO_ViewCanvas.Find("Prefeb").GetComponent<RectTransform>();

        //开关初始化
        Toggle[] Alltoggles =  GetComponentsInChildren<Toggle>();
        foreach (Toggle toggle in Alltoggles)
        {
            toggle.isOn = false;
        }

        //绑定按钮事件

        //主菜单切换按钮事件
        List<GameObject> buttons = FindAllChildObjectsByName(this.gameObject, "Buttons");
        foreach (var bu in buttons)
        {
            Button[] bus = bu.GetComponentsInChildren<Button>();
            foreach (var b in bus)
            {
                b.onClick.AddListener(() =>SwitchingInterface(b));
            }
        }
        //布局优化按钮事件
        if (CreatePSOButton != null)
            CreatePSOButton.onClick.AddListener(CreatePSOFuntion);

        Toggle[] allPrefebToggles = PSO_Prefeb.GetComponentsInChildren<Toggle>();
        ToggleGroup tg = PSO_Prefeb.GetComponent<ToggleGroup>();//开关组
        tg.allowSwitchOff = false;//是否允许所有开关关闭
        foreach (Toggle toggle in allPrefebToggles)
        {
            toggle.group = tg;
        }

        //加工设备挂载
        for (int i = 0; i < machineGameObject.Count; i++)
        {
            PSOFuntion.machineGameObject.Add(machineGameObject[i]);
        }


        //调度相关
        //调度开始按钮绑定
        if (CreateGENButton != null)
            CreateGENButton.onClick.AddListener(CreateGENFuntion);
        if (CreateGENEncodeButton != null)
            CreateGENEncodeButton.onClick.AddListener(CreateGENEncodeFuntion);

        //GENFuntion = new GEN_FunctionTest();
        GENFuntion.canvas = GENResultChart;
        GENEncoedFuntion.GEN_Function = GENFuntion;
        GENEncoedFuntion.pieceLineCanvas = PieceLineChart;
        //控件初始化挂载
        GEN_ViewCanvas = CreateGENButton.transform.parent.GetComponent<RectTransform>();
        //获取对应UI组件
        GEN_Basic = GEN_ViewCanvas.Find("Basic").GetComponent<RectTransform>();
        GEN_Simulation = GEN_ViewCanvas.Find("Simulation").GetComponent<RectTransform>();

        //动线分析相关
        Tra_Basic = TransformAnalys.transform.Find("Basic").GetComponent<RectTransform>();


        //主界面显示初始化
        SwitchingInterface(this.transform.Find("Buttons").Find("1").GetComponent<Button>());

    }
    // Update is called once per frame
    void Update()
    {
        if (GENEncoedFuntion.isActiveAndEnabled)
            GENSimulationParameter(GEN_Simulation);

        //动线分析
        if (TransformAnalys != null)
            GENEncoedFuntion.openLineAnalysis = TransformAnalys.activeInHierarchy;
        if (TransformAnalys.activeInHierarchy)
            TransformParameter(Tra_Basic);
    }
}
