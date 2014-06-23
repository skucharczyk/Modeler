using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modeler.Data.Shapes;

namespace Modeler.Undo
{
    class BezierUndoStack
    {
        public class Element
        {
            public BezierSurface bezier;
            public Element next;
        }

        public Element first = null;
        public Element firstRedo = null;
        private const int MAX_MEMORY = 10000000;

        public void Save(BezierSurface bezier)
        {
            Element second = first;
            first = new Element();
            first.next = second;
            first.bezier = new BezierSurface(bezier);

            firstRedo = null;
            int m = 0;
            for (Element el = first; el != null; el = el.next)
            {
                //m += el.scene.EstimatedMemory();
                m += el.bezier.EstimatedMemory();
                if (m > MAX_MEMORY) el.next = null;
            }
        }

        public BezierSurface Undo(BezierSurface bezier)
        {
            if (first != null)
            {
                Element secondRedo = firstRedo;
                firstRedo = new Element();
                firstRedo.next = secondRedo;
                firstRedo.bezier = new BezierSurface(bezier);

                bezier = first.bezier;
                first = first.next;
            }
            return bezier;
        }

        public BezierSurface Redo(BezierSurface bezier)
        {
            if (firstRedo != null)
            {
                Element second = first;
                first = new Element();
                first.next = second;
                first.bezier = new BezierSurface(bezier);

                bezier = firstRedo.bezier;
                firstRedo = firstRedo.next;
            }
            return bezier;
        }
    }
}
