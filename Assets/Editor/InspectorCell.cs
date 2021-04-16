using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(PCG.CellularAutomata)), CanEditMultipleObjects]
public class InspectorCell : Editor
{
    bool[] ticks =
    {
        true, true, true,
        true,       true,
        true, true, true
    };
    float birth_chance = 0.5f;
    float birth_rate = 0.0f;
    float death_rate = 0.0f;

    const float label_offset = 16.0f;
    const float size = 16.0f;
    static Vector2 offset = new Vector2(2.0f, 2.0f);
    Rect[] tick_rects =
    {
        new Rect(0 * size + offset.x, 0 * size + offset.y, size, size),
        new Rect(1 * size + offset.x, 0 * size + offset.y, size, size),
        new Rect(2 * size + offset.x, 0 * size + offset.y, size, size),
        new Rect(0 * size + offset.x, 1 * size + offset.y, size, size),
        new Rect(2 * size + offset.x, 1 * size + offset.y, size, size),
        new Rect(0 * size + offset.x, 2 * size + offset.y, size, size),
        new Rect(1 * size + offset.x, 2 * size + offset.y, size, size),
        new Rect(2 * size + offset.x, 2 * size + offset.y, size, size),
    };



    public void draw_neighbour_selection(PCG.CellularAutomata ca)
    {
        GUILayout.Label("Neighbours", EditorStyles.boldLabel);

        ca.cell_prototype = new PCG.Cell();

        var position = Vector2.zero;
        var label_pos = Rect.zero;
        label_pos.height = 16.0f;

        GUILayout.BeginHorizontal();
        int x = 0;
        for (int i = 0; i < ticks.Length; i++)
        {
            if (x > 2)
            {
                x = 0;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
            bool before_middle = i == (ticks.Length / 2) - 1;
            bool is_middle = i == ticks.Length / 2;
            if (is_middle)
                x++;
            x++;
            var pos = tick_rects[i];
            pos.x += position.x;
            pos.y += position.y + label_offset;

            GUILayoutOption[] options =
            {
                GUILayout.Width(!before_middle ? pos.width : pos.width + size + 3),
            };

            GUIContent content = new GUIContent("", "Neighbour to check for Cellular Automata");
            ticks[i] = GUILayout.Toggle(ticks[i], content, options);
            ca.cell_prototype.should_check_neighbour.ticks[i] = ticks[i];
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }

    private void draw_slider(PCG.CellularAutomata ca, string label, ref float track, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, EditorStyles.boldLabel);
        track = Mathf.Clamp(track, min, max);
        GUILayout.Label(track.ToString(), EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        track = GUILayout.HorizontalSlider(track, min, max);
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }

    public override void OnInspectorGUI()
    {
        var cell = serializedObject.FindProperty("cell_prototype");
        var reference = serializedObject.targetObject;
        var ca = (PCG.CellularAutomata)reference;

        //var ca = (PCG.CellularAutomata)target;
        draw_neighbour_selection(ca);

        draw_slider(ca, "Birth Chance:", ref birth_chance, 0, 1.0f);
        draw_slider(ca, "Death Rate:", ref birth_rate, 0, ca.cell_prototype.should_check_count());
        draw_slider(ca, "Birth Rate:", ref death_rate, 0, ca.cell_prototype.should_check_count());

        serializedObject.Update();
    }

    //public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{
    //    //base.OnGUI(position, property, label);

    //    //var tick_field = property.FindPropertyRelative("should_check_neighbour");
    //    var cell = property.objectReferenceValue;


    //    var label_pos = position;
    //    label_pos.height = 16.0f;
    //    EditorGUI.LabelField(label_pos, label_text, EditorStyles.boldLabel);
    //    for(int i = 0; i < ticks.Length; i++)
    //    {
    //        var pos = tick_rects[i];
    //        pos.x += position.x;
    //        pos.y += position.y + label_offset;
    //        ticks[i] = EditorGUI.Toggle(pos, ticks[i]);
    //    }
    //}
}
