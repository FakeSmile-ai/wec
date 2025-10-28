export function installStorageDebugging() {
  (window as any).storage = {
    clear: () => localStorage.clear(),
    get: (key: string) => localStorage.getItem(key),
    set: (key: string, value: string) => localStorage.setItem(key, value),
    all: () => ({ ...localStorage })
  };
  console.log('%c🧩 Storage debugging installed', 'color: green; font-weight: bold;');
}
