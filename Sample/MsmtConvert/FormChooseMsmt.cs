using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RanOpt.iBuilding.BLL.MeasurementsModule;
using RanOpt.iBuilding.BLL.MeasurementsModule.Data;
using RanOpt.iBuilding.BLL.MeasurementsModule.Rendering;
using RanOpt.iBuilding.BLL.PredictionDAL;
using RanOpt.iBuilding.Common;

namespace Plugin.Demo.Sample.MsmtConvert
{
    public partial class FormChooseMsmt : Form
    {
        private PredictionDataSet predList;
        private MeasurementCollection msmtList;

        public FormChooseMsmt()
        {
            InitializeComponent();
        }

        public FormChooseMsmt(PredictionDataSet predictions, MeasurementCollection msmts)
            : this()
        {
            predList = predictions;
            msmtList = msmts;
        }

        private void FormChooseMsmt_Load(object sender, EventArgs e)
        {
            var source = new List<string>();
            foreach (Measurement measurement in msmtList.Measurements)
            {
                chkListMsmt.Items.Add(measurement);
                source.AddRange(measurement.Fields);
            }
            source = source.Distinct().ToList();
            cmbRef.Items.AddRange(source.ToArray());
            foreach (PredictionDataSet.PredictionRow row in predList.Prediction)
            {
                cmbPrediction.Items.Add(row.Title);
            }
            lbxResult.Items.Add("Please select the measurement to export the prediction results.");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            lbxResult.Items.Clear();
            if (chkListMsmt.CheckedItems.Count == 0)
            {
                lbxResult.Items.Add("Please select at lease one measurement.");
            }
            if (cmbPrediction.SelectedItem == null)
            {
                lbxResult.Items.Add("Please select a prediction data.");
            }
            if (lbxResult.Items.Count > 0)
                return;

            var row = predList.Prediction[cmbPrediction.SelectedIndex];
            var field = cmbRef.SelectedItem.ToString();
            foreach (Measurement measurement in chkListMsmt.CheckedItems)
            {
                int fieldIdx = measurement.FindFieldIndex(field);
                if (fieldIdx == -1)
                    continue;
                double[] numArray;
                double[] numArray2;
                double[] numArray3;
                measurement.ConvertAllPoints(out numArray, out numArray2, out numArray3);
                if (numArray == null || numArray2 == null)
                    continue;

                // x, y, z
                var xArray = Array.ConvertAll(numArray, x => (float)x);
                var yArray = Array.ConvertAll(numArray2, y => (float)y);
                float[] zArray;
                if (numArray3 == null)
                {
                    zArray = new float[xArray.Length];
                }
                else
                {
                    zArray = Array.ConvertAll(numArray3, z => (float)z);
                }

                if (chkMetreXY.Checked)
                {
                    // append x y by metre
                    var metreFields = new List<FieldItem>
                    {
                        new FieldItem {Name = "X_Metre", Index = -1, Type = "float", Unit = "m"},
                        new FieldItem {Name = "Y_Metre", Index = -1, Type = "float", Unit = "m"}
                    };
                    SmartCheckMapFields(measurement, metreFields);

                    for (var i = 0; i < measurement.Points.Count; i++)
                    {
                        measurement.Points[i].SetValue(metreFields[0].Index, xArray[i].ToInvariantString());
                        measurement.Points[i].SetValue(metreFields[1].Index, yArray[i].ToInvariantString());
                    }
                }

                Array array;
                row.TryGetSignalData(measurement.BuildingName, xArray, yArray, zArray, out array, null);
                string str2 = "REF_VALUE";
                string str3 = "REF_DIFF";
                int num2 = measurement.FindFieldIndex(str2);
                if (num2 == -1)
                {
                    num2 = measurement.AddField(str2);
                }
                int num3 = measurement.FindFieldIndex(str3);
                if (num3 == -1)
                {
                    num3 = measurement.AddField(str3);
                }
                var type = array.GetType();
                if (type == typeof(float[]))
                {
                    Mapping mapping1 = new Mapping
                    {
                        Source = str2,
                        Target = str2,
                        Type = "float",
                        Unit = "dB"
                    };
                    measurement.MappingTable.Mappings.Add(mapping1);
                }
                else
                {
                    var mapping2 = new Mapping
                    {
                        Source = str2,
                        Target = str2,
                        Type = "string"
                    };
                    measurement.MappingTable.Mappings.Add(mapping2);
                }
                var item = new Mapping
                {
                    Source = str3,
                    Target = str3,
                    Type = "float",
                    Unit = "dB"
                };
                measurement.MappingTable.Mappings.Add(item);
                int num4 = 0;
                foreach (object obj2 in array)
                {
                    if (type == typeof(float[]))
                    {
                        float num5;
                        if (measurement.Points[num4].TryGetValue(fieldIdx, out num5, ','))
                        {
                            measurement.Points[num4].SetValue(num3,
                                Math.Abs((float)(((float)obj2) - num5)).ToString());
                        }
                        measurement.Points[num4].SetValue(num2, obj2.ToString());
                    }
                    else
                    {
                        string str4;
                        if (measurement.Points[num4].TryGetValue<string>(fieldIdx, out str4, ','))
                        {
                            measurement.Points[num4].SetValue(num3, obj2.Equals(str4).ToString());
                        }
                        measurement.Points[num4].SetValue(num2, obj2.ToString());
                    }
                    num4++;
                }
                msmtList.RaiseMsmtUpdated(measurement);
                lbxResult.Items.Add($"{measurement.Name} done, please check it in measurement data property.");
            }
            lbxResult.Items.Add("All measurements done.");
        }

        private void SmartCheckMapFields(Measurement measurement, List<FieldItem> fields)
        {
            foreach (var field in fields)
            {
                var fieldIndex = measurement.FindFieldIndex(field.Name);
                if (fieldIndex == -1)
                {
                    field.Index = measurement.AddField(field.Name);
                }

                var map = measurement.MappingTable.FindBySource(field.Name);
                if (map == null)
                {
                    map = new Mapping
                    {
                        Source = field.Name,
                        Target = field.Name,
                        Type = field.Type,
                        Unit = field.Unit
                    };
                    measurement.MappingTable.Mappings.Add(map);
                }
            }
        }

        private class FieldItem
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Unit { get; set; } = string.Empty;
            public int Index { get; set; } = -1;
        }
    }
}
