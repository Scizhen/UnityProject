using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using RDTS;

public class UIController : MonoBehaviour
{
    public GameObject Robot1;
    public GameObject Robot2;
    public GameObject Robot3;
    public GameObject Robot4;
    public Button optionsButton;
    public Button backButton;
    public VisualElement mainMenu;
    public VisualElement optionsMenu;
    public VisualElement valueMenu;
    private VisualElement root;
    private VisualElement VisualElementRobot1;
    private VisualElement VisualElementRobot2;
    private VisualElement VisualElementRobot3;
    private VisualElement VisualElementRobot4;

    private void Awake()
    {
        // 获取 rootVisualElement，它是 UI 层级开始的根视觉元素。
        root = GetComponent<UIDocument>().rootVisualElement;

        // 给变量赋值
        optionsButton = root.Q<Button>("OptionsButton");
        backButton = root.Q<Button>("BackButton");
        mainMenu = root.Q<VisualElement>("MainMenu");
        optionsMenu = root.Q<VisualElement>("OptionsMenu");
        valueMenu = root.Q<VisualElement>("ValueMenu");
        VisualElementRobot1 = root.Q<VisualElement>("Robot1");
        VisualElementRobot2 = root.Q<VisualElement>("Robot2");
        VisualElementRobot3 = root.Q<VisualElement>("Robot3");
        VisualElementRobot4 = root.Q<VisualElement>("Robot4");


        // 将以下方法指定给这些按钮；
        optionsButton.clicked += OptionsButtonPressed;
        backButton.clicked += BackButtonPressed;

        RobotValueAwake(VisualElementRobot1, Robot1);
        RobotValueAwake(VisualElementRobot2, Robot2);
        RobotValueAwake(VisualElementRobot3, Robot3);
        RobotValueAwake(VisualElementRobot4, Robot4);
    }

    void OptionsButtonPressed()
    {
        mainMenu.style.display = DisplayStyle.None;
        optionsMenu.style.display = DisplayStyle.Flex;
        valueMenu.style.display = DisplayStyle.Flex;
    }

    void BackButtonPressed()
    {
        optionsMenu.style.display = DisplayStyle.None;
        valueMenu.style.display = DisplayStyle.None;
        mainMenu.style.display = DisplayStyle.Flex;

    }
    void RobotValueAwake(VisualElement VisualElementRobot,GameObject Robot)
    {
        VisualElementRobot.Query<Label>("Robot1HeadTitle").First().text = Robot.name;
        Drive[] RobotDrive = Robot.GetComponentsInChildren<Drive>();
        for (int i = 0; i < 6;i++)
            {
            VisualElementRobot.Query<Label>("Robot1TitleLabel").AtIndex(i).text = RobotDrive[i].name;
        }

    }
    void RobotValueUpdate(VisualElement VisualElementRobot, GameObject Robot)
    {
        Drive[] RobotDrive = Robot.GetComponentsInChildren<Drive>();
        for (int i = 0; i < 6; i++)
        {
            VisualElementRobot.Query<Label>("Robot1ValueLabel").AtIndex(i).text = RobotDrive[i].CurrentPosition.ToString("f3");
        }

    }
    void Update()
    {
        RobotValueUpdate(VisualElementRobot1, Robot1);
        RobotValueUpdate(VisualElementRobot2, Robot2);
        RobotValueUpdate(VisualElementRobot3, Robot3);
        RobotValueUpdate(VisualElementRobot4, Robot4);
    }
}