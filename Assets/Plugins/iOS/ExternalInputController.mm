#import "ExternalInputController.h"

@interface ExternalInputController () <UIApplicationDelegate> {
}

@end

void UnityPause(int pause);
void UnitySetAudioSessionActive(bool active);
UIViewController *UnityGetGLViewController();

@implementation ExternalInputController

@synthesize keyInput;

+(ExternalInputController*)GameController
{
    static ExternalInputController *sharedSingleton;
    
    if(!sharedSingleton)
        sharedSingleton = [[ExternalInputController alloc] init];
    
    return sharedSingleton;
}

-(void)setupExternalInput
{
    dispatch_async(dispatch_get_main_queue(), ^{
        keyInput = [[KeyInputViewController alloc] initWithNibName:@"KeyInputViewController" bundle:[NSBundle mainBundle]];
        keyInput.view.frame = CGRectMake(0, 0, 0, 0);
        keyInput.view.backgroundColor = [UIColor clearColor];
        [UnityGetGLViewController().view addSubview:keyInput.view];
    });
}

@end


