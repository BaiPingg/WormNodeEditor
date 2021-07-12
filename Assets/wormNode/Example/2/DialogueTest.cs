using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wormNode;
using UnityEngine.UI;
using static wormNode.NodePort;
using System;
using System.Linq;
public class DialogueTest : MonoBehaviour
{
    public Graph dialogueGraph;
    public GameObject parent;
    public GameObject content; 
    public GameObject selection;
    private Node currentNode;

    private void Start()
    {
        StartDialogue();
    }
    void StartDialogue()
    {
        parent.gameObject.SetActive(true);
        for (int i = 0; i < dialogueGraph.nodes.Count; i++)
        {
            if (dialogueGraph.nodes[i] is StartNode)
            {
               
                currentNode = dialogueGraph.nodes[i].GetOutputPort("start").Connection.Node;
            }
        }

        if (currentNode)
        {
            ShowNode(currentNode);
        }

    }

    private void ShowNode(Node currentNode)
    {
        var node = currentNode as DialogueNode;
        if (node)
        {
            SetContent(node.currentContent);
            ClearOptions();

            if (currentNode.GetOutputPort("content").ConnectionCount >0)
            {
                currentNode = currentNode.GetOutputPort("content").Connection.Node;
                if (currentNode is SelectNode)
                {
                    var con = currentNode.GetOutputPort("objOut").GetPortConnections();

                    SetSelection(con);
                }
            }
           
           
        }
        //else
        //    Move2NextNode();


    }

    public void SetContent(string str)
    {
        content.transform.GetComponent<Text>().text = str;
    }
    public void SetSelection(List<PortConnection> connections)
    {
       
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].Port.Node is DialogueNode)
            {
               
                var node = (connections[i].Port.Node as DialogueNode);
               
                var obj = GameObject.Instantiate(selection.transform.Find("temp"), selection.transform.Find("options"));
                //Debug.LogError(node.currentContent);
                obj.transform.Find("Text").GetComponent<Text>().text =(i+1)+"."+ node.currentContent;
                obj.GetComponent<Button>().onClick.AddListener(() => {
                    currentNode = node;
                    Move2NextNode();
                });
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -50, 0) * i;
                obj.gameObject.SetActive(true);
            }
        }
    }

    void ClearOptions()
    {
        for (int i = 0; i < selection.transform.Find("options").childCount; i++)
        {
            GameObject.Destroy(selection.transform.Find("options").GetChild(i).gameObject);
        }
    }
    private void Move2NextNode()
    {
       
        if (currentNode is DialogueNode)
        {
            //Debug.LogError(((DialogueNode)currentNode).currentContent);
            if (currentNode.GetOutputPort("content").Connection != null)
            {
                currentNode = currentNode.GetOutputPort("content").Connection.Node;
                //Debug.LogError(currentNode.GetType());
            }
            else
            {
                currentNode = null;
            }

        }
         else if (currentNode is SelectNode)
        {
            currentNode = currentNode.GetOutputPort("objOut").Connection.Node;
        }
        else if (currentNode is StartNode)
        {
            currentNode = currentNode.GetOutputPort("start").Connection.Node;
        }

        if (currentNode)
        {
            ShowNode(currentNode);
        }
        else
        {
            parent.gameObject.SetActive(false);
        }
       
    }
}
