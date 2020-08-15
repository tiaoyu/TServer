using TServer.ECSComponent;

namespace TServer.ECSEntity
{
    public class EEntity
    {
        public int Id { get; set; }
        /// <summary> 
        /// 视野半径 格子为单位长度
        /// </summary>
        public int SightDistance { get; set; }
        /// <summary>
        /// 可被发现的半径 格子为单位长度
        /// </summary>
        public int CanBeSeeDistance { get; set; }
        /// <summary>
        /// 自动索敌半径 格子为单位长度
        /// </summary>
        public int AutoAttackDistance { get; set; }
        public CSight Sight { get; set; }
        public EEntityType EntityType { get; set; }
        public CDungeon Dungeon { get; set; }
        public CPosition<double> Position { get; set; }
        public CPosition<double> BirthPosition { get; set; }
        public CMovement Movement { get; set; }

        public EEntity()
        {
            Sight = new CSight();
            Movement = new CMovement
            {
                Speed = 5
            };
            SightDistance = 5;
        }

        public virtual void Update() { }

        public static readonly int[] BaseId = new int[4];
        public static int GenerateEntityId(EEntityType type)
        {
            return (int)type * 100000000 + ++BaseId[(int)type];
        }
        public static bool IsRole(int id)
        {
            return IsEntity(id, EEntityType.ROLE);
        }
        public static bool IsMonster(int id)
        {
            return IsEntity(id, EEntityType.MONSTER);
        }
        private static bool IsEntity(int id, EEntityType type)
        {
            return id / 100000000 == (int)type;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj as EEntity).Id.Equals(Id);
        }
    }
}
