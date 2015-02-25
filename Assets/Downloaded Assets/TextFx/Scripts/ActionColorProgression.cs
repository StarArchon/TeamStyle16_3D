#region

using System;
using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

[Serializable]
public class ActionColorProgression : ActionVariableProgression
{
	[SerializeField] private Color m_from = Color.white;
	[SerializeField] private Color m_to = Color.white;
	[SerializeField] private Color m_to_to = Color.white;
	[SerializeField] private Color[] m_values;

	public ActionColorProgression(Color start_colour)
	{
		m_from = start_colour;
		m_to = start_colour;
		m_to_to = start_colour;
	}

#if UNITY_EDITOR
	public int NumEditorLines { get { return Progression == (int)ValueProgression.Constant ? 2 : 3; } }
#endif
	public Color ValueFrom { get { return m_from; } }
	public Color[] Values { get { return m_values; } set { m_values = value; } }
	public Color ValueThen { get { return m_to_to; } }
	public Color ValueTo { get { return m_to; } }

	public void CalculateProgressions(int num_progressions, Color[] offset_cols)
	{
		if (Progression == (int)ValueProgression.Eased || Progression == (int)ValueProgression.EasedCustom || Progression == (int)ValueProgression.Random || (m_is_offset_from_last && offset_cols.Length > 1))
		{
			var constant_offset = offset_cols != null && offset_cols.Length == 1;
			m_values = new Color[num_progressions];

			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_is_offset_from_last ? offset_cols[constant_offset ? 0 : idx] : new Color(0, 0, 0, 0);
		}
		else
			m_values = new Color[1] { m_is_offset_from_last ? offset_cols[0] : new Color(0, 0, 0, 0) };

		if (Progression == (int)ValueProgression.Random) // && (progression >= 0 || m_unique_randoms))
			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] += m_from + (m_to - m_from) * Random.value;
		else if (Progression == (int)ValueProgression.Eased)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				if (m_to_to_bool)
					if (progression <= 0.5f)
						m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression / 0.5f);
					else
					{
						progression -= 0.5f;
						m_values[idx] += m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression / 0.5f);
					}
				else
					m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
			}
		}
		else if (Progression == (int)ValueProgression.EasedCustom)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				m_values[idx] += m_from + (m_to - m_from) * m_custom_ease_curve.Evaluate(progression);
			}
		}
		else if (Progression == (int)ValueProgression.Constant)
			for (var idx = 0; idx < m_values.Length; idx++)
				m_values[idx] += m_from;
	}

	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, Color[] offset_cols)
	{
		var progression_idx = GetProgressionIndex(progression_variables, animate_per);
		var constant_offset = offset_cols != null && offset_cols.Length == 1;

		m_values[progression_idx] = m_is_offset_from_last ? offset_cols[constant_offset ? 0 : progression_idx] : new Color(0, 0, 0, 0);
		m_values[progression_idx] += m_from + (m_to - m_from) * Random.value;
	}

	public ActionColorProgression Clone()
	{
		var color_progression = new ActionColorProgression(Color.white);

		color_progression.m_progression_idx = Progression;
		color_progression.m_ease_type = m_ease_type;
		color_progression.m_from = m_from;
		color_progression.m_to = m_to;
		color_progression.m_to_to = m_to_to;
		color_progression.m_to_to_bool = m_to_to_bool;
		color_progression.m_is_offset_from_last = m_is_offset_from_last;
		color_progression.m_unique_randoms = m_unique_randoms;
		color_progression.m_override_animate_per_option = m_override_animate_per_option;
		color_progression.m_animate_per = m_animate_per;

		return color_progression;
	}

#if UNITY_EDITOR
	public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		var x_offset = position.x + ACTION_INDENT_LEVEL_1;
		var y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, ProgressionExtraOptions, ProgressionExtraOptionIndexes);

		EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), Progression == (int)ValueProgression.Constant ? "Colour" : "Colour\nFrom", EditorStyles.miniLabel);
		x_offset += 60;

		m_from = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_from);

		if (Progression != (int)ValueProgression.Constant)
		{
			x_offset += 65;

			EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), "Colour\nTo", EditorStyles.miniBoldLabel);
			x_offset += 60;

			m_to = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to);

			if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
			{
				x_offset += 65;

				EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), "Colour\nThen To", EditorStyles.miniBoldLabel);
				x_offset += 60;

				m_to_to = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to_to);
			}

			if (Progression == (int)ValueProgression.EasedCustom)
			{
				m_custom_ease_curve = EditorGUI.CurveField(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset + LINE_HEIGHT + 10, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve", m_custom_ease_curve);
				y_offset += LINE_HEIGHT * 1.2f;
			}
		}

		return (y_offset + LINE_HEIGHT + 10) - position.y;
	}
#endif

	public override JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_from"] = m_from.ExportData();
		json_data["m_to"] = m_to.ExportData();
		json_data["m_to_to"] = m_to_to.ExportData();

		return new JSONValue(json_data);
	}

	public Color GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default) { return GetValue(GetProgressionIndex(progression_variables, animate_per_default)); }

	public Color GetValue(int progression_idx)
	{
		var num_vals = m_values.Length;
		if (num_vals > 1 && progression_idx < num_vals)
			return m_values[progression_idx];
		if (num_vals == 1)
			return m_values[0];
		return Color.white;
	}

	public override void ImportData(JSONObject json_data)
	{
		m_from = json_data["m_from"].Obj.JSONtoColor();
		m_to = json_data["m_to"].Obj.JSONtoColor();
		m_to_to = json_data["m_to_to"].Obj.JSONtoColor();

		ImportBaseData(json_data);
	}

	public void ImportLegacyData(string data_string)
	{
		KeyValuePair<string, string> value_pair;
		var obj_list = data_string.StringToList(';', ':');

		foreach (var obj in obj_list)
		{
			value_pair = (KeyValuePair<string, string>)obj;

			switch (value_pair.Key)
			{
				case "m_from":
					m_from = value_pair.Value.StringToColor('|', '<');
					break;
				case "m_to":
					m_to = value_pair.Value.StringToColor('|', '<');
					break;
				case "m_to_to":
					m_to_to = value_pair.Value.StringToColor('|', '<');
					break;

				default:
					ImportBaseLagacyData(value_pair);
					break;
			}
		}
	}

	public void SetConstant(Color constant_value)
	{
		m_progression_idx = (int)ValueProgression.Constant;
		m_from = constant_value;
	}

	public void SetEased(EasingEquation easing_function, Color eased_from, Color eased_to)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;
		m_ease_type = easing_function;
	}

	public void SetEased(EasingEquation easing_function, Color eased_from, Color eased_to, Color eased_then)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to = eased_then;
		m_to_to_bool = true;
		m_ease_type = easing_function;
	}

	public void SetEasedCustom(AnimationCurve easing_curve, Color eased_from, Color eased_to)
	{
		m_progression_idx = (int)ValueProgression.EasedCustom;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;

		m_custom_ease_curve = easing_curve;
	}

	public void SetRandom(Color random_min, Color random_max, bool unique_randoms = false)
	{
		m_progression_idx = (int)ValueProgression.Random;
		m_from = random_min;
		m_to = random_max;
		m_unique_randoms = unique_randoms;
	}
}