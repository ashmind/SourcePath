namespace SourcePath {
    public struct SourcePathDialectSupports {
        // inverted since struct has no default ctor, but we want defaults to be true
        private bool _noTopLevelAxis;
        private bool _noTopLevelSegments;
        private bool _noTopLevelAnd;
        private bool _noAxisSelf;
        private bool _noAxisDescendant;
        private bool _noAxisParent;
        private bool _noAxisAncestor;

        public bool TopLevelAxis {
            get { return !_noTopLevelAxis; }
            set { _noTopLevelAxis = !value; }
        }

        public bool TopLevelSegments {
            get { return !_noTopLevelSegments; }
            set { _noTopLevelSegments = !value; }
        }

        public bool TopLevelAnd {
            get { return !_noTopLevelAnd; }
            set { _noTopLevelAnd = !value; }
        }

        public bool AxisSelf {
            get { return !_noAxisSelf; }
            set { _noAxisSelf = !value; }
        }

        public bool AxisDescendant {
            get { return !_noAxisDescendant; }
            set { _noAxisDescendant = !value; }
        }

        public bool AxisParent {
            get { return !_noAxisParent; }
            set { _noAxisParent = !value; }
        }

        public bool AxisAncestor {
            get { return !_noAxisAncestor; }
            set { _noAxisAncestor = !value; }
        }
    }
}
