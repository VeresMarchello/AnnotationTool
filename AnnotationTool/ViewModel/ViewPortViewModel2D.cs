using AnnotationTool.Model;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Collections.Generic;
using System.Windows;
using Media3D = System.Windows.Media.Media3D;


namespace AnnotationTool.ViewModel
{
    public class ViewPortViewModel2D
    {
        //private MeshGeometry3D _plane;
        //private PhongMaterial _planeMaterial;
        //private Media3D.Transform3D _planeTransform;

        //private LineGeometry3D _lines;
        //private _2DLine _selected2dLine;
        //private List<_2DLine> _2dLineList;
        //private LineGeometry3D _newLine;


        //public _2DLine Selected2dLine
        //{
        //    get { return _selected2dLine; }
        //    set
        //    {
        //        _selected2dLine = value;
        //        NotifyPropertyChanged();

        //        if (value != null)
        //        {
        //            var target = GetVectorFromPixel(value.CenterPoint);
        //            SetCameraTarget(target);
        //        }
        //    }
        //}
        //public List<_2DLine> _2DLineList
        //{
        //    get { return _2dLineList; }
        //    set
        //    {
        //        _2dLineList = value;
        //        NotifyPropertyChanged();
        //    }
        //}


        //public Vector3 FirstPoint { get; set; }
        //public bool IsFirstPoint { get; set; }

        public ViewPortViewModel2D()
        {
            //var box = new MeshBuilder();
            //box.AddBox(new Vector3(0, 0, 0), 10, 10, 0, BoxFaces.PositiveZ);
            //_plane = box.ToMeshGeometry3D();
            //_planeMaterial = PhongMaterials.Blue;
            //_planeTransform = new Media3D.TranslateTransform3D(0, 0, 0);
        }

        //public MeshGeometry3D Plane
        //{
        //    get { return _plane; }
        //    set
        //    {
        //        _plane = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //public PhongMaterial PlaneMaterial
        //{
        //    get { return _planeMaterial; }
        //    set
        //    {
        //        _planeMaterial = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //public Media3D.Transform3D PlaneTransform
        //{
        //    get { return _planeTransform; }
        //    set
        //    {
        //        _planeTransform = value;
        //        NotifyPropertyChanged();
        //    }
        //}
    }
}
