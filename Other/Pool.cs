using System;
using System.Collections.Generic;

public class Pool {
	
		//IMPORTANTE: Esta clase NO puede ser modificada

        private Func<IPoolObject>   _create;
        private Action<IPoolObject> _disable;

        private List<IPoolObject> _uninstantiated;


        public Pool(Func<IPoolObject> createFunction, Action<IPoolObject> disableFunction, int initialQuantity) {
            _create  = createFunction;
            _disable = disableFunction;

            _uninstantiated = new List<IPoolObject>();

            for (var i = 0; i < initialQuantity; i++) {
                var obj = _create(); 
                _disable(obj);
                _uninstantiated.Add(obj);
            }
        }

        public IPoolObject AcquireObject(){
            if (_uninstantiated.Count == 0) {
                return _create();
            }
            var obj = _uninstantiated[_uninstantiated.Count - 1];
            _uninstantiated.Remove(obj);
            return obj;
        }

        public void ReleaseObject(IPoolObject obj){
            _disable(obj);
            _uninstantiated.Add(obj);
        }

}