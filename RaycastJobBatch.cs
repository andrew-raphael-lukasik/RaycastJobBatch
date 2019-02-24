using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public struct RaycastJobBatch : System.IDisposable
{
	#region FIELDS

	
	public NativeArray<RaycastCommand> commands;
	public NativeArray<RaycastHit> results;
	JobHandle _handle;
	public JobHandle handle => _handle;

	Allocator _allocator;

	
	#endregion
	#region PROPERTIES

	
	public int Length
	{
		get
		{
			int numCommands = this.commands.Length;
			int numResults = this.results.Length;
			return numCommands==numResults ? numCommands : -1;//-1 when array lengths are not equal
		}
		set
		{
			if( this.Length!=value )
			{
				this.Dispose();
				this.commands = new NativeArray<RaycastCommand>( value , _allocator );
				this.results = new NativeArray<RaycastHit>( value , _allocator , NativeArrayOptions.UninitializedMemory );
			}
		}
	}


	#endregion
	#region CONSTRUCTORS
	

	public RaycastJobBatch ( int length , Allocator allocator )
	{
		this._allocator = allocator;
		this.commands = new NativeArray<RaycastCommand>( length , _allocator );
		this.results = new NativeArray<RaycastHit>( length , _allocator , NativeArrayOptions.UninitializedMemory );
		this._handle = default(JobHandle);
	}

	
	#endregion
	#region PUBLIC METHODS


	public void Schedule ( int minCommandsPerJob = 32 , JobHandle dependsOn = default(JobHandle) )
	{
		#if DEBUG
		if( _handle.IsCompleted==false ) Debug.LogWarning( "Scheduling when job is not complete" );
		#endif
		this._handle = RaycastCommand.ScheduleBatch( this.commands , this.results , minCommandsPerJob , dependsOn );
	}

	public void Complete ()
	{
		this._handle.Complete();
		this._handle = default(JobHandle);
	}

	public void CopyResults ( ref RaycastHit[] array )
	{
		#if DEBUG
		if( _handle.IsCompleted==false ) Debug.LogWarning( "Copying results when job is not complete" );
		#endif
		if( array.Length!=this.results.Length )
		{
			System.Array.Resize( ref array , this.results.Length );
		}
		this.results.CopyTo( array );
	}

	public void Dispose ()
	{
		this.Complete();
		if( this.commands.IsCreated ) { this.commands.Dispose(); }
		if( this.results.IsCreated ) { this.results.Dispose(); }
	}


	#endregion
}
