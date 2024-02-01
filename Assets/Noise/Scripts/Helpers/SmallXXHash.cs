using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Noise {
public readonly struct SmallXXHash {
	const uint primeA = 0b10011110001101110111100110110001;
	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;
	
	readonly public uint accumulator;

	public SmallXXHash(uint accumulator){
		this.accumulator = accumulator;
	}

	public SmallXXHash Eat(int data) => RotateLeft(accumulator + ((uint) data * primeC), 17) * primeD;
	public SmallXXHash Eat(byte data) => RotateLeft(accumulator + data*primeE, 11) * primeA;
	public uint ByteA() => (uint) this & 0xFF;
	public uint ByteB() => ((uint) this >> 8) & 0xFF;
	public uint ByteC() => ((uint) this >> 16) & 0xFF;
	public uint ByteD() => (uint) this >> 24;
	public float Float01A() => (float) ByteA() * (1f / 255f);
	public float Float01B() => (float) ByteB() * (1f / 255f);
	public float Float01C() => (float) ByteC() * (1f / 255f);
	public float Float01D() => (float) ByteD() * (1f / 255f);
	public uint GetBits(int shift, int count) => ((uint) this >> shift) & (uint) ((1 << count) - 1);
	public float GetBitsAsFloat01(int shift, int count) => (float) GetBits(shift, count) * (1f / ((1 << count) - 1)); 

	public static SmallXXHash Seed(uint seed) => new SmallXXHash((uint) seed + primeE);
	public static SmallXXHash Select(SmallXXHash ifFalse, SmallXXHash ifTrue, bool condition) => 
		math.select(ifFalse.accumulator, ifTrue.accumulator, condition);

	public static SmallXXHash operator + (SmallXXHash hash, int v) => hash.accumulator + (uint) v;
	public static implicit operator SmallXXHash(uint accumulator) => new SmallXXHash(accumulator);
	public static implicit operator uint (SmallXXHash hash){
		uint avalanche = hash.accumulator;
		avalanche ^= avalanche >> 15;
		avalanche *= primeB;
		avalanche ^= avalanche >> 13;
		avalanche *= primeC;
		avalanche ^= avalanche >> 16; 
		return avalanche;
	}

	// There is no rotate vectorized instruction so we use two bit shifts 
	// and OR to rotate since these instructions does have vectorization
	static uint RotateLeft (uint data, int steps) => (data << steps) | (data >> 32 - steps);
}

public readonly struct SmallXXHash4 {

	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;
	
	readonly public uint4 accumulator;

	public SmallXXHash4(uint4 accumulator){
		this.accumulator = accumulator;
	}

	public SmallXXHash4 Eat(int4 data) => RotateLeft(accumulator + ((uint4) data * primeC), 17) * primeD;
	public uint4 ByteA() => (uint4) this & 0xFF;
	public uint4 ByteB() => ((uint4) this >> 8) & 0xFF;
	public uint4 ByteC() => ((uint4) this >> 16) & 0xFF;
	public uint4 ByteD() => (uint4) this >> 24;
	public float4 Float01A() => (float4) ByteA() * (1f / 255f);
	public float4 Float01B() => (float4) ByteB() * (1f / 255f);
	public float4 Float01C() => (float4) ByteC() * (1f / 255f);
	public float4 Float01D() => (float4) ByteD() * (1f / 255f);
	public uint4 GetBits(int shift, int count) => ((uint4) this >> shift) & (uint) ((1 << count) - 1);
	public float4 GetBitsAsFloat01(int shift, int count) => (float4) GetBits(shift, count) * (1f / ((1 << count) - 1));

	public static SmallXXHash4 Seed(uint4 seed) => new SmallXXHash4((uint4) seed + primeE);
	public static SmallXXHash4 Select(SmallXXHash4 ifFalse, SmallXXHash4 ifTrue, bool4 condition) => 
		math.select(ifFalse.accumulator, ifTrue.accumulator, condition);

	public static SmallXXHash4 operator + (SmallXXHash4 hash, int v) => hash.accumulator + (uint) v;
	public static implicit operator SmallXXHash4(uint4 accumulator) => new SmallXXHash4(accumulator);
	public static implicit operator SmallXXHash4(SmallXXHash hash) => new SmallXXHash4(hash.accumulator);
	public static implicit operator uint4 (SmallXXHash4 hash){
		uint4 avalanche = hash.accumulator;
		avalanche ^= avalanche >> 15;
		avalanche *= primeB;
		avalanche ^= avalanche >> 13;
		avalanche *= primeC;
		avalanche ^= avalanche >> 16; 
		return avalanche;
	}

	// There is no rotate vectorized instruction so we use two bit shifts 
	// and OR to rotate since these instructions does have vectorization
	static uint4 RotateLeft (uint4 data, int steps) => (data << steps) | (data >> 32 - steps);

}}
