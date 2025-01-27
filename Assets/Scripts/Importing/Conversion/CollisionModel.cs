using SanAndreasUnity.Importing.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Profiling;

namespace SanAndreasUnity.Importing.Conversion
{
    public class CollisionModel
    {
        private static UnityEngine.Vector3 Convert(Vector3 vec)
        {
            return new UnityEngine.Vector3(vec.X, vec.Z, vec.Y);
        }

		private static Mesh Convert(IEnumerable<Face> faces, int numFaces, IEnumerable<Vertex> vertices, int numVertices)
        {
			Profiler.BeginSample ("Convert mesh");

			// create vertices array for the mesh
			UnityEngine.Vector3[] meshVertices = new UnityEngine.Vector3[numVertices];
			int i = 0;
			foreach (var v in vertices) {
				meshVertices [i] = Convert (v.Position);
				i++;
			}

            var mesh = new Mesh
            {
				vertices = meshVertices,
                subMeshCount = 1
            };

			// indices

			//var indices = faces.SelectMany(x => x.GetIndices()).ToArray();

			// each face has 3 indices which form a single triangle, and we should also add another one
			// which faces the opposite direction
			int[] indices = new int[numFaces * 3 * 2];

			i = 0;
			foreach (var f in faces) {
				indices [i++] = f.A;
				indices [i++] = f.B;
				indices [i++] = f.C;

				// triangle with opposite direction
				indices [i++] = f.B;
				indices [i++] = f.A;
				indices [i++] = f.C;
			}

			mesh.SetIndices(indices, MeshTopology.Triangles, 0);


			Profiler.EndSample ();

            return mesh;
        }

		private static Mesh Convert(FaceGroup group, ICollection<Face> faces, ICollection<Vertex> vertices)
        {
			int numFaces = 1 + group.EndFace - group.StartFace;
			return Convert(faces.Skip(group.StartFace).Take(numFaces), numFaces, vertices, vertices.Count);
        }

        private static GameObject _sTemplateParent;

        private static readonly Dictionary<string, CollisionModel> _sLoaded
            = new Dictionary<string, CollisionModel>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// 加载碰撞器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="destParent"></param>
        /// <param name="forceConvex"></param>
        public static void Load(string name, Transform destParent, bool forceConvex = false)
        {
            Load(name, null, destParent, forceConvex);
        }

        /// <summary>
        /// 加载碰撞器
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destParent"></param>
        /// <param name="forceConvex"></param>
        public static void Load(CollisionFile file, Transform destParent, bool forceConvex = false)
        {
            Load(file.Name, file, destParent, forceConvex);
        }

        /// <summary>
        /// 加载碰撞器文件，添加碰撞器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="destParent"></param>
        /// <param name="forceConvex"></param>
        private static void Load(string name, CollisionFile file, Transform destParent, bool forceConvex)
        {
            CollisionModel col;

            if (_sLoaded.ContainsKey(name))
            {
                col = _sLoaded[name];
                if (col == null) return;

                col.Spawn(destParent, forceConvex);
                return;
            }

            file = file ?? CollisionFile.FromName(name);
            if (file == null || (file.Flags & Flags.NotEmpty) != Flags.NotEmpty)
            {
                _sLoaded.Add(name, null);
                return;
            }

            col = new CollisionModel(file);
            _sLoaded.Add(name, col);

            col.Spawn(destParent, forceConvex);
        }

		public static void LoadAsync(string name, CollisionFile file, Transform destParent, bool forceConvex, float loadPriority, System.Action onFinish)
		{
			// load collision file asyncly, and when it's ready just call the other function

			if (file != null)
			{
				// collision file already loaded
				// just call other function

				UGameCore.Utilities.F.RunExceptionSafe( () => Load(file, destParent, forceConvex) );
				onFinish ();
				return;
			}

			// load collision file asyncly
			CollisionFile.FromNameAsync (name, loadPriority, (cf) => {
				// loading finished
				// call other function
				if(cf != null)
                    UGameCore.Utilities.F.RunExceptionSafe( () => Load( cf, destParent, forceConvex ) );
				onFinish ();
			});

		}


        private readonly GameObject _template;
        private readonly Dictionary<SurfaceFlags, Transform> _flagGroups;

        /// <summary>
        /// 给物体添加不同类型碰撞器
        /// </summary>
        /// <typeparam name="TCollider"></typeparam>
        /// <param name="surface"></param>
        /// <param name="setup"></param>
        private void Add<TCollider>(Surface surface, Action<TCollider> setup)
            where TCollider : Collider
        {
			Profiler.BeginSample ("Add<" + typeof(TCollider).Name + ">");

            if (!_flagGroups.ContainsKey(surface.Flags))
            {
                var group = new GameObject(string.Format("Group {0}", (int)surface.Flags));
                group.transform.SetParent(_template.transform);

                _flagGroups.Add(surface.Flags, group.transform);
            }

            var type = typeof(TCollider);
            //这里添加了一个子物体数量的后缀，用于区分不同名称
            var obj = new GameObject(type.Name+"_"+ _flagGroups[surface.Flags].childCount, type);
            obj.transform.SetParent(_flagGroups[surface.Flags]);

            setup(obj.GetComponent<TCollider>());

			Profiler.EndSample ();
        }

        /// <summary>
        /// 生成碰撞器物体模板预制体，
        /// 这些物体都会生成在Collision Templates物体下
        /// </summary>
        /// <param name="file"></param>
        private CollisionModel(CollisionFile file)
        {
			Profiler.BeginSample ("CollisionModel()");

            if (_sTemplateParent == null)
            {
                _sTemplateParent = new GameObject("Collision Templates");
                _sTemplateParent.SetActive(false);
            }
            _template = new GameObject(file.Name);
            _template.transform.SetParent(_sTemplateParent.transform);

            _flagGroups = new Dictionary<SurfaceFlags, Transform>();

            //添加BoxCollider
            foreach (var box in file.Boxes)
            {
                Add<BoxCollider>(box.Surface, x =>
                {
                    var min = Convert(box.Min);
                    var max = Convert(box.Max);

                    x.center = (min + max) * .5f;
                    x.size = (max - min);
                });
            }

            //添加SphereCollider
            foreach (var sphere in file.Spheres)
            {
                Add<SphereCollider>(sphere.Surface, x =>
                {
                    x.center = Convert(sphere.Center);
                    x.radius = sphere.Radius;
                });
            }

            //添加MeshCollider
            if (file.FaceGroups.Length > 0)
            {
                foreach (var group in file.FaceGroups)
                {
                    Add<MeshCollider>(file.Faces[group.StartFace].Surface, x =>
                    {
                        x.sharedMesh = Convert(group, file.Faces, file.Vertices);
                    });
                }
            }
            else if (file.Faces.Length > 0)
            {
                Add<MeshCollider>(file.Faces[0].Surface, x =>
                {
					x.sharedMesh = Convert(file.Faces, file.Faces.Length, file.Vertices, file.Vertices.Length);
                });
            }

            // TODO: MeshCollider


			Profiler.EndSample ();
        }

        /// <summary>
        /// 生成碰撞器物体
        /// </summary>
        /// <param name="destParent"></param>
        /// <param name="forceConvex"></param>
        public void Spawn(Transform destParent, bool forceConvex)
        {
            Debug.Log($"gcj: ____Spawn AAA _template=null : {_template == null}, _template name: {_template.name}");
            var clone = Object.Instantiate(_template.gameObject);

            clone.name = "Collision";
            clone.transform.SetParent(destParent, false);

            //	Debug.Log ("Setting parent (" + destParent.name + ") for " + clone.name);

            if (!forceConvex) return;

			Profiler.BeginSample ("Adjust colliders");

            foreach (var collider in clone.GetComponentsInChildren<Collider>())
            {
                var meshCollider = collider as MeshCollider;

                collider.gameObject.layer = 2;

                if (meshCollider != null)
                {
                    meshCollider.convex = true;
                }
            }

			Profiler.EndSample ();
        }
    }
}
