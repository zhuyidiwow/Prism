using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class PostBuild {
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path) {
        #if UNITY_IOS
            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetName = PBXProject.GetUnityTargetName();
            
            string targetGUID = project.TargetGuidByName(targetName);

            AddFrameworks(project, targetGUID);

            File.WriteAllText(projectPath, project.WriteToString());
        #endif
    }

    #if UNITY_IOS
    private static void AddFrameworks(PBXProject project, string targetGUID) {
        project.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
        project.AddFrameworkToProject(targetGUID, "CoreData.framework", false);
        project.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);
        project.AddFrameworkToProject(targetGUID, "libz.tbd", false);
        project.AddFrameworkToProject(targetGUID, "libsqlite3.tbd", false);
    }
    #endif
}